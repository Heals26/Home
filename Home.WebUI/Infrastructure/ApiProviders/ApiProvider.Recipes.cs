using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetRecipeBaseUrl(long recipeID)
        => $"{GetRecipesBaseUrl()}/{recipeID}";

    private static string GetRecipesBaseUrl()
        => $"{GetBaseApiUrl()}/Recipes";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateRecipe()
        => new(HttpMethod.Post, RouteType.Body, GetRecipesBaseUrl());

    public static ApiProviderHelper DeleteRecipe(long recipeID)
        => new(HttpMethod.Delete, RouteType.Route, GetRecipeBaseUrl(recipeID));

    public static ApiProviderHelper GetRecipe(long recipeID)
        => new(HttpMethod.Get, RouteType.Route, GetRecipeBaseUrl(recipeID));

    public static ApiProviderHelper GetRecipes()
        => new(HttpMethod.Get, RouteType.Route, GetRecipesBaseUrl());

    public static ApiProviderHelper ImportRecipe()
        => new(HttpMethod.Post, RouteType.Body, $"{GetRecipesBaseUrl()}/Import");

    public static ApiProviderHelper UpdateRecipe(long recipeID)
        => new(HttpMethod.Patch, RouteType.Body, GetRecipeBaseUrl(recipeID));

    #endregion Methods

}
