using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.MealPlanEntries.CreateMealPlanEntry;
using Home.WebUI.DataAccess.MealPlanEntries.GetMealPlanEntries;
using Home.WebUI.DataAccess.MealPlanEntries.Models;
using Home.WebUI.DataAccess.Recipes.GetRecipes;
using Home.WebUI.DataAccess.ShoppingLists.AddMealPlanToShoppingList;
using Home.WebUI.DataAccess.ShoppingLists.CreateShoppingList;
using Home.WebUI.DataAccess.ShoppingLists.GetShoppingLists;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;

namespace Home.WebUI.Components.Pages.MealPlan;

public partial class MealPlanPage : IDisposable
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;

    // Loaded data
    private ICollection<MealPlanEntryDto>? m_Entries;
    private ICollection<GetRecipeDto>? m_Recipes;
    private ICollection<GetShoppingListDto>? m_ShoppingLists;
    private DateTime m_WeekStart;

    // Picker modal
    private bool m_ShowPicker;
    private DateTime m_PickerDate;
    private bool m_Planning;

    // Add-to-list modal
    private bool m_ShowAddToList;
    private bool m_AddingToList;
    private string m_NewListName = string.Empty;
    private int? m_AddedRecipeCount;

    #endregion Fields

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        this.m_WeekStart = StartOfWeek(this.TimeProvider.GetLocalNow().Date);

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
            await this.LoadWeekAsync();
            this.StateHasChanged();
        });
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

    private async Task OpenPickerAsync(DateTime date)
    {
        this.m_PickerDate = date;
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
            new CreateMealPlanEntryWebAppRequest() { Date = this.m_PickerDate, RecipeID = recipeID },
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
            new AddMealPlanToShoppingListWebAppRequest() { FromDate = this.m_WeekStart, ToDate = this.m_WeekStart.AddDays(6) },
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

    // Helpers

    private IEnumerable<MealPlanEntryDto> EntriesFor(DateTime day)
        => (this.m_Entries ?? []).Where(e => e.Date.Date == day);

    private bool IsCurrentWeek()
        => this.m_WeekStart == StartOfWeek(this.TimeProvider.GetLocalNow().Date);

    private static DateTime StartOfWeek(DateTime date)
        => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

    private IEnumerable<DateTime> WeekDays()
        => Enumerable.Range(0, 7).Select(i => this.m_WeekStart.AddDays(i));

    private string WeekLabel()
        => $"{this.m_WeekStart:d MMM} – {this.m_WeekStart.AddDays(6):d MMM}";

    #endregion Methods

}
