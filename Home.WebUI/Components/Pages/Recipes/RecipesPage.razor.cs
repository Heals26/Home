using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.Recipes.CreateRecipe;
using Home.WebUI.DataAccess.Recipes.GetRecipes;
using Home.WebUI.DataAccess.Recipes.ImportRecipe;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Home.WebUI.Infrastructure.Services.ChangeNotifications;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Recipes;

public partial class RecipesPage : IDisposable
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private IDisposable? m_ChangeSubscription;
    private GetRecipesWebAppResponse? m_Recipes;
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
        await this.LoadRecipesAsync();

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
        if (area != ChangeArea.Recipes)
            return;

        await this.InvokeAsync(async () =>
        {
            await this.LoadRecipesAsync();
            this.StateHasChanged();
        });
    }

    private void OpenCreateModal()
    {
        this.m_CreateRequest = new();
        this.m_ImportUrl = string.Empty;
        this.m_ShowCreate = true;
    }

    private void OpenRecipe(long recipeID)
        => this.NavigationManager.NavigateTo($"/recipes/{recipeID}");

    private async Task LoadRecipesAsync()
    {
        var _Result = await this.ApiAccess.SendRequestAsync<object, GetRecipesWebAppResponse>(
            null!, ApiProvider.GetRecipes(),
            e => this.m_ErrorHandler?.AddError(e),
            this.m_CancellationTokenHandler.Token);

        if (_Result != null)
            this.m_Recipes = _Result;
    }

    private async Task CreateRecipeAsync()
    {
        if (this.m_Creating) return;
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

    #endregion Methods

}
