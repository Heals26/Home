using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetRecipeImageBaseUrl(long recipeID)
        => $"{GetBaseApiUrl()}/Recipes/{recipeID}/Image";

    #endregion Base

    #region Methods

    public static ApiProviderHelper DeleteRecipeImage(long recipeID)
        => new(HttpMethod.Delete, RouteType.Route, GetRecipeImageBaseUrl(recipeID));

    public static ApiProviderHelper GetRecipeImage(long recipeID)
        => new(HttpMethod.Get, RouteType.Route, GetRecipeImageBaseUrl(recipeID));

    public static ApiProviderHelper SetRecipeImage(long recipeID)
        => new(HttpMethod.Put, RouteType.Form, GetRecipeImageBaseUrl(recipeID));

    #endregion Methods

}
