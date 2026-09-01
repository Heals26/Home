using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.MealPlanEntries.CreateMealPlanEntry;
using Home.WebUI.DataAccess.MealPlanEntries.GetMealPlanEntries;
using Home.WebUI.DataAccess.MealPlanEntries.Models;
using Home.WebUI.DataAccess.MealPlanEntries.UpdateMealPlanEntry;
using Home.WebUI.DataAccess.MealSlots.CreateMealSlot;
using Home.WebUI.DataAccess.MealSlots.GetMealSlots;
using Home.WebUI.DataAccess.MealSlots.Models;
using Home.WebUI.DataAccess.MealSlots.UpdateMealSlot;
using Home.WebUI.DataAccess.Recipes.CreateRecipe;
using Home.WebUI.DataAccess.Recipes.GetRecipes;
using Home.WebUI.DataAccess.Recipes.SetRecipeMealSlots;
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
    private MealPlanEntryDto? m_DraggedEntry;
    private bool m_MovingEntry;
    private bool m_ShowPicker;
    private DateTime m_PickerDate;
    private long? m_PickerMealSlotID;
    private string m_PickerSearch = string.Empty;
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
        this.m_PickerSearch = string.Empty;
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

    /// <summary>
    /// Recipes tagged with the meal being planned come first — someone filling in Tuesday's
    /// breakfast is almost always choosing among breakfasts.
    /// </summary>
    private IEnumerable<GetRecipeDto> PickerRecipes()
    {
        var _Search = this.m_PickerSearch.Trim();

        return (this.m_Recipes ?? [])
            .Where(r => _Search.Length == 0 || r.Name.Contains(_Search, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(this.SuitsPickedMeal)
            .ThenBy(r => r.Name);
    }

    private bool SuitsPickedMeal(GetRecipeDto recipe)
        => this.m_PickerMealSlotID != null && recipe.MealSlots.Any(m => m.MealSlotID == this.m_PickerMealSlotID);

    private string PickedMealName()
        => this.m_MealSlots.FirstOrDefault(m => m.MealSlotID == this.m_PickerMealSlotID)?.Name ?? string.Empty;

    private bool PickerSearchMatchesExactly()
        => (this.m_Recipes ?? []).Any(r => string.Equals(r.Name, this.m_PickerSearch.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Makes the typed name a real recipe — tagged with the meal being planned, so it sorts to
    /// the top next time — then plans it into the cell that started all this.
    /// </summary>
    private async Task CreateAndPlanRecipeAsync()
    {
        var _Name = this.m_PickerSearch.Trim();

        if (this.m_Planning || _Name.Length == 0)
            return;

        this.m_Planning = true;

        var _Created = await this.ApiAccess.SendRequestAsync<CreateRecipeWebAppRequest, CreateRecipeWebAppResponse>(
            new CreateRecipeWebAppRequest() { Name = _Name },
            ApiProvider.CreateRecipe(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Created == null)
        {
            this.m_Planning = false;
            return;
        }

        if (this.m_PickerMealSlotID != null)
            _ = await this.ApiAccess.SendRequestAsync<SetRecipeMealSlotsWebAppRequest, bool>(
                new SetRecipeMealSlotsWebAppRequest() { MealSlotIDs = [this.m_PickerMealSlotID.Value] },
                ApiProvider.SetRecipeMealSlots(_Created.RecipeID),
                e => this.m_ErrorHandler?.AddError(e),
                this.m_CancellationTokenHandler.Token);

        // The book changed, so the cached picker list is stale for the next open.
        this.m_Recipes = null;
        this.m_Planning = false;

        await this.ChangeBroadcaster.PublishAsync(ChangeArea.Recipes, this.m_CancellationTokenHandler.Token);
        await this.PlanRecipeAsync(_Created.RecipeID);
    }

    /// <summary>
    /// Enter in the picker does the obvious thing: plan the one recipe left after filtering, or
    /// create what was typed when nothing matches. With nothing typed there is nothing to submit.
    /// </summary>
    private async Task SubmitPickerAsync()
    {
        if (this.m_PickerSearch.Trim().Length == 0)
            return;

        var _Matches = this.PickerRecipes().ToList();

        if (_Matches.Count > 0 && this.PickerSearchMatchesExactly())
            await this.PlanRecipeAsync(_Matches[0].RecipeID);
        else if (_Matches.Count == 1)
            await this.PlanRecipeAsync(_Matches[0].RecipeID);
        else
            await this.CreateAndPlanRecipeAsync();
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

    // Moving a planned meal

    /// <summary>
    /// Nudges a meal a day either way, keeping the meal of the day it was on. This is the path a
    /// finger uses; dragging is the same move with the target picked directly.
    /// </summary>
    private async Task ShiftEntryAsync(MealPlanShift shift)
        => await this.MoveEntryAsync(shift.Entry, shift.Entry.Date.AddDays(shift.Days), shift.Entry.MealSlotID);

    private void StartDraggingEntry(MealPlanEntryDto entry)
        => this.m_DraggedEntry = entry;

    private async Task DropEntryAsync(DateTime date, long? mealSlotID)
    {
        if (this.m_DraggedEntry is not { } _Entry)
            return;

        this.m_DraggedEntry = null;

        await this.MoveEntryAsync(_Entry, date, mealSlotID);
    }

    private async Task MoveEntryAsync(MealPlanEntryDto entry, DateTime date, long? mealSlotID)
    {
        // A move onto the square it already occupies is not a move, and a round trip that changes
        // nothing would still repaint the week under the family.
        if (this.m_MovingEntry || (entry.Date.Date == date.Date && entry.MealSlotID == mealSlotID))
            return;

        this.m_MovingEntry = true;

        var _Result = await this.ApiAccess.SendRequestAsync<UpdateMealPlanEntryWebAppRequest, bool>(
            new UpdateMealPlanEntryWebAppRequest()
            {
                Date = new PropertyChangeTracker<DateTime>(date.Date),
                MealSlotID = new PropertyChangeTracker<long?>(mealSlotID)
            },
            ApiProvider.UpdateMealPlanEntry(entry.MealPlanEntryID),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        this.m_MovingEntry = false;

        if (_Result != true)
            return;

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
