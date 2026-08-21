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

        var _Stored = await this.ReadStoredViewAsync();

        if (_Stored == null || _Stored == this.m_View)
            return;

        this.m_View = _Stored;
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

    private IEnumerable<GetRecipeDto> OrderedRecipes()
        => (this.m_Recipes?.Recipes ?? []).OrderBy(r => r.Name);

    private string DescribeMealSlots(IEnumerable<RecipeMealSlotDto> mealSlots)
        => string.Join(" · ", mealSlots.OrderBy(m => m.Sequence).Select(m => m.Name));

    private string EmptyTitle()
        => this.m_MealSlotFilter == null ? "No recipes yet" : "Nothing for that meal yet";

    private string EmptySubtitle()
        => this.m_MealSlotFilter == null
            ? "Add your first recipe to get the book started"
            : "No recipe has been marked as suiting this meal";

    #endregion Methods

}
