using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.Components.Shared.Inputs;
using Home.WebUI.DataAccess.MealSlots.GetMealSlots;
using Home.WebUI.DataAccess.MealSlots.Models;
using Home.WebUI.DataAccess.Recipes.CreateRecipe;
using Home.WebUI.DataAccess.Recipes.GetRecipes;
using Home.WebUI.DataAccess.Recipes.ImportRecipe;
using Home.WebUI.DataAccess.Recipes.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Recipes;

public partial class RecipesPage : IDisposable
{

    #region Fields

    /// <summary>
    /// Which layout this device was last left on. Card or list is a per-tablet preference, not a
    /// household one, so it lives in the browser rather than on the household row.
    /// </summary>
    private const string ViewStorageKey = "home.recipes.view";

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;

    private GetRecipesWebAppResponse? m_Recipes;
    private List<MealSlotDto> m_MealSlots = [];
    private long? m_MealSlotFilter;

    /// <summary>
    /// How the book is ordered. A per-device preference like the layout — the tablet in the
    /// kitchen and a phone in the aisle want different things out of the same book.
    /// </summary>
    private const string SortStorageKey = "home.recipes.sort";

    private string m_Search = string.Empty;
    private string m_Sort = "name";
    private int? m_MaxMinutes;

    private string m_View = "cards";
    private readonly List<HomeSegmentedControl<string>.SegmentOption> m_ViewOptions =
    [
        new("Cards", "cards"),
        new("List", "list")
    ];

    private CreateRecipeWebAppRequest? m_CreateRequest = new();
    private bool m_ShowCreate;
    private bool m_Creating;
    private string m_ImportUrl = string.Empty;
    private bool m_Importing;

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await this.LoadMealSlotsAsync();
        await this.LoadRecipesAsync();

        this.m_ChangeSubscription = await this.ChangeBroadcaster.SubscribeAsync(
            this.OnHouseholdChangedAsync, this.m_CancellationTokenHandler.Token);
    }

    /// <summary>
    /// Protected browser storage needs a live circuit, so the stored layout can only be read
    /// once the first render has happened.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        var _StoredView = await this.ReadStoredViewAsync();
        var _StoredSort = await this.ReadStoredSortAsync();

        var _Changed = false;

        if (_StoredView != null && _StoredView != this.m_View)
        {
            this.m_View = _StoredView;
            _Changed = true;
        }

        if (_StoredSort != null && _StoredSort != this.m_Sort)
        {
            this.m_Sort = _StoredSort;
            _Changed = true;
        }

        if (_Changed)
            this.StateHasChanged();
    }

    public void Dispose()
    {
        this.m_ChangeSubscription?.Dispose();
        this.m_CancellationTokenHandler.Dispose();
    }

    #endregion Lifecycle Methods

    #region Methods

    private async Task OnHouseholdChangedAsync(ChangeArea area)
    {
        if (area != ChangeArea.Recipes)
            return;

        await this.InvokeAsync(async () =>
        {
            await this.LoadMealSlotsAsync();
            await this.LoadRecipesAsync();
            this.StateHasChanged();
        });
    }

    private async Task LoadMealSlotsAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetMealSlotsWebAppResponse>(
            null!, ApiProvider.GetMealSlots(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result == null)
            return;

        this.m_MealSlots = [.. _Result.MealSlots.OrderBy(m => m.Sequence).ThenBy(m => m.Name)];

        // A meal deleted on another device must not leave the book filtered to nothing.
        if (this.m_MealSlotFilter != null && !this.m_MealSlots.Any(m => m.MealSlotID == this.m_MealSlotFilter))
            this.m_MealSlotFilter = null;
    }

    private async Task LoadRecipesAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetRecipesWebAppResponse>(
            null!, ApiProvider.GetRecipes(this.m_MealSlotFilter),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Recipes = _Result;
    }

    private async Task FilterByMealAsync(long? mealSlotID)
    {
        if (this.m_MealSlotFilter == mealSlotID)
            return;

        this.m_MealSlotFilter = mealSlotID;
        this.m_Recipes = null;

        await this.LoadRecipesAsync();
    }

    private async Task SetViewAsync(string view)
    {
        this.m_View = view;

        try
        {
            await this.LocalStorage.SetAsync(ViewStorageKey, view);
        }
        catch
        {
            // Storage being unavailable costs the preference, never the page.
        }
    }

    private async Task<string?> ReadStoredViewAsync()
    {
        try
        {
            var _Result = await this.LocalStorage.GetAsync<string>(ViewStorageKey);

            return _Result.Success && _Result.Value is "cards" or "list" ? _Result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> ReadStoredSortAsync()
    {
        try
        {
            var _Result = await this.LocalStorage.GetAsync<string>(SortStorageKey);

            return _Result.Success && _Result.Value is "name" or "quickest" or "simplest" ? _Result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    private void OpenCreateModal()
    {
        this.m_CreateRequest = new();
        this.m_ImportUrl = string.Empty;
        this.m_ShowCreate = true;
    }

    private void OpenRecipe(long recipeID)
        => this.NavigationManager.NavigateTo($"/recipes/{recipeID}");

    /// <summary>
    /// The modal has two ways in, so Enter follows whichever the user actually filled in — a
    /// pasted address imports, a typed name creates.
    /// </summary>
    private async Task SubmitCreateModalAsync()
    {
        if (!string.IsNullOrWhiteSpace(this.m_ImportUrl))
            await this.ImportRecipeAsync();
        else
            await this.CreateRecipeAsync();
    }

    private async Task CreateRecipeAsync()
    {
        if (this.m_Creating || string.IsNullOrWhiteSpace(this.m_CreateRequest!.Name)) return;
        this.m_Creating = true;

        var _Result = await this.ApiAccess.SendRequestAsync<CreateRecipeWebAppRequest, CreateRecipeWebAppResponse>(
            this.m_CreateRequest!,
            ApiProvider.CreateRecipe(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Creating = false;

        if (_Result == null)
            return;

        this.m_ShowCreate = false;

        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Recipes, this.m_CancellationTokenHandler.Token);

        this.NavigationManager.NavigateTo($"/recipes/{_Result.RecipeID}");
    }

    private async Task ImportRecipeAsync()
    {
        if (this.m_Importing || string.IsNullOrWhiteSpace(this.m_ImportUrl))
            return;

        this.m_Importing = true;

        var _Result = await this.ApiAccess.SendRequestAsync<ImportRecipeWebAppRequest, ImportRecipeWebAppResponse>(
            new ImportRecipeWebAppRequest() { Url = this.m_ImportUrl.Trim() },
            ApiProvider.ImportRecipe(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Importing = false;

        if (_Result == null)
            return;

        this.m_ShowCreate = false;

        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Recipes, this.m_CancellationTokenHandler.Token);

        this.NavigationManager.NavigateTo($"/recipes/{_Result.RecipeID}");
    }

    private async Task DeleteRecipeAsync(long recipeID)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteRecipe(recipeID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != true)
            return;

        await this.LoadRecipesAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Recipes, this.m_CancellationTokenHandler.Token);
    }

    // Display

    /// <summary>
    /// The book as this device is currently asking to see it: matching the search, inside the time
    /// the cook has, in the order they chose.
    /// </summary>
    private IEnumerable<GetRecipeDto> VisibleRecipes()
    {
        var _Recipes = (this.m_Recipes?.Recipes ?? []).AsEnumerable();
        var _Search = this.m_Search.Trim();

        if (_Search.Length > 0)
            _Recipes = _Recipes.Where(r => r.Name.Contains(_Search, StringComparison.OrdinalIgnoreCase));

        // A recipe that has never been timed is not hidden by a time filter — it might well be
        // quick, and dropping it would quietly shrink the book for a value nobody entered.
        if (this.m_MaxMinutes is { } _MaxMinutes)
            _Recipes = _Recipes.Where(r => TotalMinutes(r) is not { } _Minutes || _Minutes <= _MaxMinutes);

        return this.m_Sort switch
        {
            // Untimed and unjudged recipes sort last rather than first, because a missing value is
            // not the same as a quick or a simple one.
            "quickest" => _Recipes.OrderBy(r => TotalMinutes(r) ?? int.MaxValue).ThenBy(r => r.Name),
            "simplest" => _Recipes.OrderBy(r => r.Complexity ?? long.MaxValue).ThenBy(r => r.Name),
            _ => _Recipes.OrderBy(r => r.Name)
        };
    }

    /// <summary>
    /// Prep plus cook, or null when the household has timed neither — how long the recipe takes
    /// from starting to eating.
    /// </summary>
    private static int? TotalMinutes(GetRecipeDto recipe)
        => recipe.PrepMinutes == null && recipe.CookMinutes == null
            ? null
            : (recipe.PrepMinutes ?? 0) + (recipe.CookMinutes ?? 0);

    private void SetSearch(string search)
        => this.m_Search = search;

    private async Task SetSortAsync(ChangeEventArgs args)
    {
        this.m_Sort = args.Value?.ToString() ?? "name";

        // A dead circuit cannot remember anything, and that is never worth an error on screen.
        try { await this.LocalStorage.SetAsync(SortStorageKey, this.m_Sort); } catch { }
    }

    private void SetMaxMinutes(ChangeEventArgs args)
        => this.m_MaxMinutes = int.TryParse(args.Value?.ToString(), out var _Minutes) ? _Minutes : null;

    private string DescribeMealSlots(IEnumerable<RecipeMealSlotDto> mealSlots)
        => string.Join(" · ", mealSlots.OrderBy(m => m.Sequence).Select(m => m.Name));

    /// <summary>
    /// Whether anything is narrowing the book. An empty book and a book with nothing matching are
    /// different situations and get different words — and only one of them wants "Add recipe".
    /// </summary>
    private bool IsNarrowed()
        => this.m_MealSlotFilter != null || this.m_MaxMinutes != null || this.m_Search.Trim().Length > 0;

    private string EmptyTitle()
    {
        if (!this.IsNarrowed())
            return "No recipes yet";

        return this.m_Search.Trim().Length > 0
            ? $"Nothing matching “{this.m_Search.Trim()}”"
            : "Nothing matches that";
    }

    private string EmptySubtitle()
    {
        if (!this.IsNarrowed())
            return "Add your first recipe to get the book started";

        if (this.m_MaxMinutes is { } _MaxMinutes && this.m_Search.Trim().Length == 0 && this.m_MealSlotFilter == null)
            return $"Nothing in the book is ready inside {_MaxMinutes} minutes";

        return "Try a shorter search, or widen the meal and time filters";
    }

    /// <summary>
    /// Clears everything narrowing the book at once — an empty screen should take one tap to get
    /// out of, not three.
    /// </summary>
    private async Task ShowEveryRecipeAsync()
    {
        this.m_Search = string.Empty;
        this.m_MaxMinutes = null;

        await this.FilterByMealAsync(null);
    }

    #endregion Methods

}
