using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetRecipeNotesBaseUrl()
        => $"{GetBaseApiUrl()}/RecipeNotes";

    #endregion Base

    #region Methods

    public static ApiProviderHelper AddRecipeNote()
        => new(HttpMethod.Post, RouteType.Body, GetRecipeNotesBaseUrl());

    public static ApiProviderHelper RemoveRecipeNote(long recipeID, long noteID)
        => new(HttpMethod.Delete, RouteType.Route, $"{GetRecipeNotesBaseUrl()}/{recipeID}/{noteID}");

    public static ApiProviderHelper UpdateNote(long noteID)
        => new(HttpMethod.Patch, RouteType.Body, $"{GetRecipeNotesBaseUrl()}/{noteID}");

    #endregion Methods

}
