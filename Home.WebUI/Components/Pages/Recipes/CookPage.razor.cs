using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.Recipes.GetRecipe;
using Home.WebUI.DataAccess.Recipes.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace Home.WebUI.Components.Pages.Recipes;

public partial class CookPage : IAsyncDisposable
{

    #region Records

    private sealed class CookTimer
    {
        public required DateTimeOffset EndsAt { get; init; }
        public bool HasChimed { get; set; }
        public bool IsFinished { get; set; }
        public required string Label { get; init; }
    }

    #endregion Records

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;

    private GetRecipeWebAppResponse? m_Recipe;
    private int m_StepIndex;

    private readonly List<CookTimer> m_Timers = [];
    private bool m_TickerRunning;

    private bool m_ShowTimerPicker;
    private string m_CustomTimerMinutes = string.Empty;

    // Durations like "20 minutes", "1 hr", "45 min" pulled from step text become one-tap timers.
    private static readonly Regex s_Durations = new(
        @"(\d+)\s*(hours?|hrs?|minutes?|mins?)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly int[] s_TimerPresets = [1, 3, 5, 10, 15, 20, 30, 45, 60];

    #endregion Fields

    #region Properties

    [Parameter] public long RecipeID { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetRecipeWebAppResponse>(
            null!, ApiProvider.GetRecipe(this.RecipeID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Recipe = _Result;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await this.JS.InvokeVoidAsync("homeCook.keepAwake");
    }

    public async ValueTask DisposeAsync()
    {
        // Cancelling ends the ticker's next wait, so no countdown outlives the page.
        this.m_CancellationTokenHandler.Dispose();
        this.m_Timers.Clear();

        try
        {
            await this.JS.InvokeVoidAsync("homeCook.release");
        }
        catch (JSDisconnectedException)
        {
            // The circuit is gone — the browser released the wake lock with the page.
        }
        catch (OperationCanceledException)
        {
            // Teardown cancelled the interop call; the lock goes with the page anyway.
        }
    }

    #endregion Lifecycle Methods

    #region Methods

    // Steps: index 0 is the ingredients overview when the recipe has ingredients.

    private IReadOnlyList<RecipeStepDto> OrderedSteps()
        => [.. (this.m_Recipe?.Steps ?? []).OrderBy(s => s.Sequence)];

    private bool HasIngredientsPage()
        => this.m_Recipe?.Ingredients.Count > 0;

    private bool OnIngredients()
        => this.HasIngredientsPage() && this.m_StepIndex == 0;

    private RecipeStepDto? CurrentStep()
    {
        var _Steps = this.OrderedSteps();
        var _Index = this.HasIngredientsPage() ? this.m_StepIndex - 1 : this.m_StepIndex;

        return _Index >= 0 && _Index < _Steps.Count ? _Steps[_Index] : null;
    }

    private int TotalPages()
        => this.OrderedSteps().Count + (this.HasIngredientsPage() ? 1 : 0);

    private bool IsLastStep()
        => this.m_StepIndex >= this.TotalPages() - 1;

    private string StepLabel()
        => this.TotalPages() == 0
            ? "Nothing written down yet"
            : this.OnIngredients()
                ? "Before you start"
                : $"Step {this.m_StepIndex + (this.HasIngredientsPage() ? 0 : 1)} of {this.OrderedSteps().Count}";

    private void LeaveCooking()
        => this.NavigationManager.NavigateTo($"/recipes/{this.RecipeID}");

    // Timers

    private void OpenTimerPicker()
    {
        this.m_CustomTimerMinutes = string.Empty;
        this.m_ShowTimerPicker = true;
    }

    private void StartPresetTimer(int minutes)
    {
        this.StartTimer(minutes);
        this.m_ShowTimerPicker = false;
    }

    private bool CanStartCustomTimer()
        => int.TryParse(this.m_CustomTimerMinutes, out var _Minutes) && _Minutes is > 0 and <= 24 * 60;

    private void StartCustomTimer()
    {
        if (!this.CanStartCustomTimer())
            return;

        this.StartTimer(int.Parse(this.m_CustomTimerMinutes));
        this.m_CustomTimerMinutes = string.Empty;
        this.m_ShowTimerPicker = false;
    }

    private IEnumerable<int> CurrentStepDurations()
    {
        var _Step = this.CurrentStep();

        if (_Step == null)
            return [];

        return s_Durations.Matches(_Step.Content)
            .Select(m =>
            {
                var _Value = int.Parse(m.Groups[1].Value);
                return m.Groups[2].Value.StartsWith('h') || m.Groups[2].Value.StartsWith('H')
                    ? _Value * 60
                    : _Value;
            })
            .Where(minutes => minutes is > 0 and <= 24 * 60)
            .Distinct()
            .Take(4);
    }

    private void StartTimer(int minutes)
    {
        this.m_Timers.Add(new CookTimer()
        {
            EndsAt = this.TimeProvider.GetUtcNow().AddMinutes(minutes),
            Label = FormatMinutes(minutes)
        });

        if (!this.m_TickerRunning)
            _ = this.TickAsync();
    }

    private void DismissTimer(CookTimer timer)
        => _ = this.m_Timers.Remove(timer);

    /// <summary>
    /// One shared ticker while any timer runs — it re-renders each second for the countdowns
    /// and chimes once per timer as it finishes.
    /// </summary>
    private async Task TickAsync()
    {
        this.m_TickerRunning = true;

        try
        {
            using var _Timer = new PeriodicTimer(TimeSpan.FromSeconds(1), this.TimeProvider);

            while (this.m_Timers.Count > 0 && await _Timer.WaitForNextTickAsync(this.m_CancellationTokenHandler.Token))
            {
                var _Now = this.TimeProvider.GetUtcNow();

                foreach (var _CookTimer in this.m_Timers.Where(t => !t.HasChimed && t.EndsAt <= _Now))
                {
                    _CookTimer.IsFinished = true;
                    _CookTimer.HasChimed = true;

                    try
                    {
                        await this.JS.InvokeVoidAsync("homeCook.chime");
                    }
                    catch (JSDisconnectedException)
                    {
                        // No circuit, no chime — the visual pulse still shows on reconnect.
                    }
                }

                await this.InvokeAsync(this.StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
            // Leaving the page cancels the token; the ticker ends with it.
        }
        finally
        {
            this.m_TickerRunning = false;
        }
    }

    private string Remaining(CookTimer timer)
    {
        var _Remaining = timer.EndsAt - this.TimeProvider.GetUtcNow();

        return _Remaining <= TimeSpan.Zero ? "0:00" : $"{(int)_Remaining.TotalMinutes}:{_Remaining.Seconds:00}";
    }

    // Formatting

    private static string FormatMinutes(int minutes)
        => minutes switch
        {
            < 60 => $"{minutes} min",
            60 => "1 hour",
            _ when minutes % 60 == 0 => $"{minutes / 60} hours",
            _ => $"{minutes / 60} h {minutes % 60} min"
        };

    private static string IngredientAmount(RecipeIngredientDto ingredient)
    {
        var _Amount = RecipeDisplayLogic.DescribeAmount(ingredient);

        return string.IsNullOrEmpty(_Amount) ? string.Empty : $" — {_Amount}";
    }

    #endregion Methods

}
