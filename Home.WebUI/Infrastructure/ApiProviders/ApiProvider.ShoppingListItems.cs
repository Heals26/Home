using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetShoppingListItemBaseUrl(long shoppingListItemID)
        => $"{GetShoppingListItemsBaseUrl()}/{shoppingListItemID}";

    private static string GetShoppingListItemsBaseUrl()
        => $"{GetBaseApiUrl()}/ShoppingListItems";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateShoppingListItem()
        => new(HttpMethod.Post, RouteType.Body, GetShoppingListItemsBaseUrl());

    public static ApiProviderHelper DeleteShoppingListItem(long shoppingListItemID)
        => new(HttpMethod.Delete, RouteType.Route, GetShoppingListItemBaseUrl(shoppingListItemID));

    public static ApiProviderHelper GetShoppingListItemSuggestions()
        => new(HttpMethod.Get, RouteType.Route, $"{GetShoppingListItemsBaseUrl()}/Suggestions");

    public static ApiProviderHelper MoveShoppingListItem(long shoppingListItemID, long shoppingListID)
        => new(HttpMethod.Put, RouteType.Route, $"{GetShoppingListItemBaseUrl(shoppingListItemID)}/List/{shoppingListID}");

    public static ApiProviderHelper SetShoppingListItemCategory(long shoppingListItemID)
        => new(HttpMethod.Put, RouteType.Body, $"{GetShoppingListItemBaseUrl(shoppingListItemID)}/Category");

    public static ApiProviderHelper UpdateShoppingListItem(long shoppingListItemID)
        => new(HttpMethod.Patch, RouteType.Body, GetShoppingListItemBaseUrl(shoppingListItemID));

    #endregion Methods

}
