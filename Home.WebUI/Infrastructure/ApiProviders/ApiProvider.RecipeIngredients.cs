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

    /// <summary>
    /// Everything the household has cooked with, fetched once when the recipe opens and filtered
    /// on the device — what a family cooks with does not change between keystrokes.
    /// </summary>
    public static ApiProviderHelper GetIngredientSuggestions()
        => new(HttpMethod.Get, RouteType.Route, $"{GetRecipeIngredientsBaseUrl()}/suggestions");

    public static ApiProviderHelper RemoveRecipeIngredient(long recipeID, long ingredientID)
        => new(HttpMethod.Delete, RouteType.Route, $"{GetRecipeIngredientsBaseUrl()}/{recipeID}/{ingredientID}");

    public static ApiProviderHelper SetRecipeIngredientSequence(long recipeID, long ingredientID)
        => new(HttpMethod.Patch, RouteType.Body, $"{GetRecipeIngredientsBaseUrl()}/{recipeID}/{ingredientID}/sequence");

    public static ApiProviderHelper UpdateRecipeIngredient(long ingredientID)
        => new(HttpMethod.Patch, RouteType.Body, $"{GetRecipeIngredientsBaseUrl()}/{ingredientID}");

    #endregion Methods

}
