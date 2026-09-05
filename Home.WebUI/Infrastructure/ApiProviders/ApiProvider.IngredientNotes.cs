using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetIngredientNotesBaseUrl()
        => $"{GetBaseApiUrl()}/IngredientNotes";

    #endregion Base

    #region Methods

    public static ApiProviderHelper AddIngredientNote()
        => new(HttpMethod.Post, RouteType.Body, GetIngredientNotesBaseUrl());

    public static ApiProviderHelper RemoveIngredientNote(long ingredientID, long noteID)
        => new(HttpMethod.Delete, RouteType.Route, $"{GetIngredientNotesBaseUrl()}/{ingredientID}/Note/{noteID}");

    /// <summary>
    /// Editing a note goes through the controller that owns it, so an ingredient note is patched
    /// here rather than on the recipe-note route, even though both end in the same interactor.
    /// </summary>
    public static ApiProviderHelper UpdateIngredientNote(long noteID)
        => new(HttpMethod.Patch, RouteType.Body, $"{GetIngredientNotesBaseUrl()}/{noteID}");

    #endregion Methods

}
