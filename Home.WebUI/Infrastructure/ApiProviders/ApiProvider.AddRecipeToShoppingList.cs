using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Methods

    public static ApiProviderHelper AddRecipeToShoppingList(long shoppingListID, long recipeID)
        => new(HttpMethod.Post, RouteType.Route, $"{GetShoppingListsBaseUrl()}/{shoppingListID}/Recipes/{recipeID}");

    #endregion Methods

}
