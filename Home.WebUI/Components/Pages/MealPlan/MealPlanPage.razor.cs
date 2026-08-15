using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.MealPlanEntries.CreateMealPlanEntry;
using Home.WebUI.DataAccess.MealPlanEntries.GetMealPlanEntries;
using Home.WebUI.DataAccess.MealPlanEntries.Models;
using Home.WebUI.DataAccess.MealSlots.CreateMealSlot;
using Home.WebUI.DataAccess.MealSlots.GetMealSlots;
using Home.WebUI.DataAccess.MealSlots.Models;
using Home.WebUI.DataAccess.MealSlots.UpdateMealSlot;
using Home.WebUI.DataAccess.Recipes.GetRecipes;
using Home.WebUI.DataAccess.ShoppingLists.AddMealPlanToShoppingList;
using Home.WebUI.DataAccess.ShoppingLists.CreateShoppingList;
using Home.WebUI.DataAccess.ShoppingLists.GetShoppingLists;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.ChangeTrackers;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using System.Globalization;

namespace Home.WebUI.Components.Pages.MealPlan;

public partial class MealPlanPage : IDisposable
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;

    // Loaded data
    private ICollection<MealPlanEntryDto>? m_Entries;
    private List<MealSlotDto> m_MealSlots = [];
    private ICollection<GetRecipeDto>? m_Recipes;
    private ICollection<GetShoppingListDto>? m_ShoppingLists;
    private DateTime m_WeekStart;

    // Picker modal
    private bool m_ShowPicker;
    private DateTime m_PickerDate;
    private long? m_PickerMealSlotID;
    private bool m_Planning;

    // Add-to-list modal
    private bool m_ShowAddToList;
    private bool m_AddingToList;
    private string m_ListMealSlotFilter = string.Empty;
    private string m_NewListName = string.Empty;
    private int? m_AddedRecipeCount;

    // Manage meals modal
    private bool m_ShowManageMeals;
    private string m_NewMealName = string.Empty;
    private string m_NewMealStartsAt = string.Empty;
    private bool m_SavingMeal;

    #endregion Fields

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        this.m_WeekStart = StartOfWeek(this.TimeProvider.GetLocalNow().Date);

        await this.LoadMealSlotsAsync();
        await this.LoadWeekAsync();

        this.m_ChangeSubscription = await this.ChangeBroadcaster.SubscribeAsync(
            this.OnHouseholdChangedAsync, this.m_CancellationTokenHandler.Token);
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
        if (area != ChangeArea.MealPlan && area != ChangeArea.Recipes)
            return;

        await this.InvokeAsync(async () =>
        {
            this.m_Recipes = area == ChangeArea.Recipes ? null : this.m_Recipes;
            await this.LoadMealSlotsAsync();
            await this.LoadWeekAsync();
            this.StateHasChanged();
        });
    }

    private async Task LoadMealSlotsAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetMealSlotsWebAppResponse>(
            null!, ApiProvider.GetMealSlots(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_MealSlots = [.. _Result.MealSlots.OrderBy(m => m.Sequence).ThenBy(m => m.Name)];
    }

    private async Task LoadWeekAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetMealPlanEntriesWebAppResponse>(
            null!, ApiProvider.GetMealPlanEntries(this.m_WeekStart, this.m_WeekStart.AddDays(6)),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Entries = _Result.Entries;
    }

    private async Task MoveWeekAsync(int days)
    {
        this.m_WeekStart = this.m_WeekStart.AddDays(days);
        this.m_Entries = null;

        await this.LoadWeekAsync();
    }

    private async Task GoToTodayAsync()
    {
        this.m_WeekStart = StartOfWeek(this.TimeProvider.GetLocalNow().Date);
        this.m_Entries = null;

        await this.LoadWeekAsync();
    }

    // Planning

    private async Task OpenPickerAsync(DateTime date, long? mealSlotID)
    {
        this.m_PickerDate = date;
        this.m_PickerMealSlotID = mealSlotID ?? this.m_MealSlots.FirstOrDefault()?.MealSlotID;
        this.m_ShowPicker = true;

        if (this.m_Recipes != null)
            return;

        var _Result = await this.ApiAccess.SendRequestAsync<object, GetRecipesWebAppResponse>(
            null!, ApiProvider.GetRecipes(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Recipes = _Result.Recipes;
    }

    private async Task PlanRecipeAsync(long recipeID)
    {
        if (this.m_Planning)
            return;

        this.m_Planning = true;

        var _Result = await this.ApiAccess.SendRequestAsync<CreateMealPlanEntryWebAppRequest, CreateMealPlanEntryWebAppResponse>(
            new CreateMealPlanEntryWebAppRequest()
            {
                Date = this.m_PickerDate,
                MealSlotID = this.m_PickerMealSlotID,
                RecipeID = recipeID
            },
            ApiProvider.CreateMealPlanEntry(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_Planning = false;

        if (_Result == null)
            return;

        this.m_ShowPicker = false;

        await this.LoadWeekAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.MealPlan, this.m_CancellationTokenHandler.Token);
    }

    private async Task DeleteEntryAsync(MealPlanEntryDto entry)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteMealPlanEntry(entry.MealPlanEntryID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != true)
            return;

        _ = this.m_Entries?.Remove(entry);
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.MealPlan, this.m_CancellationTokenHandler.Token);
    }

    // Shopping

    private async Task OpenAddToListModal()
    {
        this.m_AddedRecipeCount = null;
        this.m_ShowAddToList = true;

        var _Result = await this.ApiAccess.SendRequestAsync<object, GetShoppingListsWebAppResponse>(
            null!, ApiProvider.GetShoppingLists(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_ShoppingLists = _Result.ShoppingLists;
    }

    private async Task AddWeekToListAsync(long shoppingListID)
    {
        if (this.m_AddingToList)
            return;

        this.m_AddingToList = true;

        var _Result = await this.ApiAccess.SendRequestAsync<AddMealPlanToShoppingListWebAppRequest, AddMealPlanToShoppingListWebAppResponse>(
            new AddMealPlanToShoppingListWebAppRequest()
            {
                FromDate = this.m_WeekStart,
                MealSlotID = ParseLong(this.m_ListMealSlotFilter),
                ToDate = this.m_WeekStart.AddDays(6)
            },
            ApiProvider.AddMealPlanToShoppingList(shoppingListID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_AddingToList = false;
        this.m_AddedRecipeCount = _Result?.RecipeCount;

        if (_Result != null)
            await this.ChangeBroadcaster.PublishAsync(ChangeArea.ShoppingLists, this.m_CancellationTokenHandler.Token);
    }

    private async Task CreateListAndAddWeekAsync()
    {
        if (this.m_AddingToList || string.IsNullOrWhiteSpace(this.m_NewListName))
            return;

        this.m_AddingToList = true;

        var _Created = await this.ApiAccess.SendRequestAsync<CreateShoppingListWebAppRequest, CreateShoppingListWebAppResponse>(
            new CreateShoppingListWebAppRequest() { Name = this.m_NewListName },
            ApiProvider.CreateShoppingList(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_AddingToList = false;

        if (_Created == null)
            return;

        this.m_NewListName = string.Empty;

        await this.OpenAddToListModal();
        await this.AddWeekToListAsync(_Created.ShoppingListID);
    }

    // The household's meal vocabulary

    private void OpenManageMealsModal()
    {
        this.m_NewMealName = string.Empty;
        this.m_NewMealStartsAt = string.Empty;
        this.m_ShowManageMeals = true;
    }

    private async Task CreateMealSlotAsync()
    {
        if (this.m_SavingMeal || string.IsNullOrWhiteSpace(this.m_NewMealName))
            return;

        this.m_SavingMeal = true;

        var _Result = await this.ApiAccess.SendRequestAsync<CreateMealSlotWebAppRequest, CreateMealSlotWebAppResponse>(
            new CreateMealSlotWebAppRequest()
            {
                Name = this.m_NewMealName.Trim(),
                StartsAt = ParseTimeOfDay(this.m_NewMealStartsAt)
            },
            ApiProvider.CreateMealSlot(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_SavingMeal = false;

        if (_Result == null)
            return;

        this.m_NewMealName = string.Empty;
        this.m_NewMealStartsAt = string.Empty;

        await this.ReloadMealsAndPublishAsync();
    }

    /// <summary>
    /// Reordering renumbers the whole list rather than swapping a pair, so a run of moves can
    /// never leave two meals sharing a sequence.
    /// </summary>
    private async Task MoveMealSlotAsync(MealSlotDto mealSlot, int direction)
    {
        var _Index = this.m_MealSlots.IndexOf(mealSlot);
        var _Target = _Index + direction;

        if (_Index < 0 || _Target < 0 || _Target >= this.m_MealSlots.Count)
            return;

        this.m_MealSlots.RemoveAt(_Index);
        this.m_MealSlots.Insert(_Target, mealSlot);

        for (var _Sequence = 0; _Sequence < this.m_MealSlots.Count; _Sequence++)
        {
            var _MealSlot = this.m_MealSlots[_Sequence];

            if (_MealSlot.Sequence == _Sequence)
                continue;

            _MealSlot.Sequence = _Sequence;

            _ = await this.ApiAccess.SendRequestAsync<UpdateMealSlotWebAppRequest, bool>(
                new UpdateMealSlotWebAppRequest() { Sequence = new PropertyChangeTracker<int>(_Sequence) },
                ApiProvider.UpdateMealSlot(_MealSlot.MealSlotID),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token);
        }

        await this.ChangeBroadcaster.PublishAsync(ChangeArea.MealPlan, this.m_CancellationTokenHandler.Token);
    }

    private async Task DeleteMealSlotAsync(long mealSlotID)
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, bool>(
            null!, ApiProvider.DeleteMealSlot(mealSlotID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result == true)
            await this.ReloadMealsAndPublishAsync();
    }

    private async Task ReloadMealsAndPublishAsync()
    {
        await this.LoadMealSlotsAsync();
        await this.LoadWeekAsync();
        await this.ChangeBroadcaster.PublishAsync(ChangeArea.MealPlan, this.m_CancellationTokenHandler.Token);
    }

    // Helpers

    private IEnumerable<MealPlanEntryDto> EntriesFor(DateTime day, long? mealSlotID)
        => (this.m_Entries ?? []).Where(e => e.Date.Date == day && e.MealSlotID == mealSlotID);

    private bool HasUnassignedEntries()
        => (this.m_Entries ?? []).Any(e => e.MealSlotID == null);

    private bool IsCurrentWeek()
        => this.m_WeekStart == StartOfWeek(this.TimeProvider.GetLocalNow().Date);

    private bool IsToday(DateTime day)
        => day == this.TimeProvider.GetLocalNow().Date;

    private string DescribeStartsAt(TimeSpan? startsAt)
        => startsAt == null ? string.Empty : DateTime.MinValue.Add(startsAt.Value).ToString("h:mm tt").ToLowerInvariant();

    private string PickerTitle()
    {
        var _MealSlot = this.m_MealSlots.FirstOrDefault(m => m.MealSlotID == this.m_PickerMealSlotID);

        return _MealSlot == null
            ? $"Plan for {this.m_PickerDate:dddd d MMMM}"
            : $"{_MealSlot.Name} on {this.m_PickerDate:dddd d MMMM}";
    }

    private static long? ParseLong(string value)
        => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var _Parsed) ? _Parsed : null;

    private static TimeSpan? ParseTimeOfDay(string value)
        => TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var _Parsed) ? _Parsed : null;

    private static DateTime StartOfWeek(DateTime date)
        => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

    private IEnumerable<DateTime> WeekDays()
        => Enumerable.Range(0, 7).Select(i => this.m_WeekStart.AddDays(i));

    private string WeekLabel()
        => $"{this.m_WeekStart:d MMM} – {this.m_WeekStart.AddDays(6):d MMM}";

    #endregion Methods

}
