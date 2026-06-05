using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetRecipeIngredientsBaseUrl()
        => $"{GetBaseApiUrl()}/RecipeIngredients";

    #endregion Base

    #region Methods

    public static ApiProviderHelper AddRecipeIngredient()
        => new(HttpMethod.Post, RouteType.Body, GetRecipeIngredientsBaseUrl());

    public static ApiProviderHelper RemoveRecipeIngredient(long recipeID, long ingredientID)
        => new(HttpMethod.Delete, RouteType.Route, $"{GetRecipeIngredientsBaseUrl()}/{recipeID}/{ingredientID}");

    public static ApiProviderHelper UpdateRecipeIngredient(long ingredientID)
        => new(HttpMethod.Patch, RouteType.Body, $"{GetRecipeIngredientsBaseUrl()}/{ingredientID}");

    #endregion Methods

}
