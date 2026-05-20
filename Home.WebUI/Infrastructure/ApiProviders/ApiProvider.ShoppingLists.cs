using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetShoppingListBaseUrl(long shoppingListID)
        => $"{GetShoppingListsBaseUrl()}/{shoppingListID}";

    private static string GetShoppingListsBaseUrl()
        => $"{GetBaseApiUrl()}/ShoppingLists";

    #endregion Bases

    #region Methods

    public static ApiProviderHelper CreateShoppingList()
        => new(HttpMethod.Post, RouteType.Body, GetShoppingListsBaseUrl());

    public static ApiProviderHelper DeleteShoppingList(long shoppingListID)
        => new(HttpMethod.Delete, RouteType.Route, GetShoppingListBaseUrl(shoppingListID));

    public static ApiProviderHelper GetShoppingLists()
        => new(HttpMethod.Get, RouteType.Route, GetShoppingListsBaseUrl());

    public static ApiProviderHelper GetShoppingList(long shoppingListID)
        => new(HttpMethod.Get, RouteType.Route, GetShoppingListBaseUrl(shoppingListID));

    public static ApiProviderHelper UpdateShoppingList(long shoppingListID)
        => new(HttpMethod.Patch, RouteType.Body, GetShoppingListBaseUrl(shoppingListID));

    #endregion Methods

}
