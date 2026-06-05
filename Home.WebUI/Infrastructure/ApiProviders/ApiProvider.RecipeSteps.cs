using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetRecipeStepsBaseUrl()
        => $"{GetBaseApiUrl()}/RecipeSteps";

    #endregion Base

    #region Methods

    public static ApiProviderHelper AddRecipeStep()
        => new(HttpMethod.Post, RouteType.Body, GetRecipeStepsBaseUrl());

    public static ApiProviderHelper RemoveRecipeStep(long recipeStepID)
        => new(HttpMethod.Delete, RouteType.Route, $"{GetRecipeStepsBaseUrl()}/{recipeStepID}");

    public static ApiProviderHelper UpdateRecipeStep(long recipeStepID)
        => new(HttpMethod.Patch, RouteType.Body, $"{GetRecipeStepsBaseUrl()}/{recipeStepID}");

    #endregion Methods

}
