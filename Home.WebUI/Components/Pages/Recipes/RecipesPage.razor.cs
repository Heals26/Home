using Home.WebUI.Components.Pages.Shared.ErrorHandlers;
using Home.WebUI.DataAccess.Recipes.CreateRecipe;
using Home.WebUI.DataAccess.Recipes.GetRecipes;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.CancellationTokens;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Recipes;

public partial class RecipesPage
{

    #region Fields

    private CancellationTokenHandler m_CancellationTokenHandler = new();
    private ErrorHandler? m_ErrorHandler;
    private GetRecipesWebAppResponse? m_Recipes;
    private CreateRecipeWebAppRequest? m_CreateRequest = new();
    private bool m_ShowCreate;
    private bool m_Creating;

    #endregion Fields

    #region Properties

    [CascadingParameter(Name = "CancellationToken")] public CancellationToken CancellationToken { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
        => await this.LoadRecipesAsync();

    #endregion Lifecycle Methods

    #region Methods

    private void OpenCreateModal()
    {
        this.m_CreateRequest = new();
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
    }

    #endregion Methods

}
