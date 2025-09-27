using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Bases

    private static string GetShoppingCartBaseUrl(long shoppingCartID)
        => $"{GetShoppingCartsBaseUrl()}/{shoppingCartID}";

    private static string GetShoppingCartsBaseUrl()
        => "ShoppingCarts";

    #endregion Bases

    #region Methods

    public static ApiProviderHelper CreateShoppingCart()
        => new(HttpMethod.Post, RouteType.Body, GetShoppingCartsBaseUrl());

    public static ApiProviderHelper DeleteShoppingCart(long shoppingCartID)
        => new(HttpMethod.Delete, RouteType.Route, GetShoppingCartBaseUrl(shoppingCartID));

    public static ApiProviderHelper GetShoppingCarts()
        => new(HttpMethod.Get, RouteType.Route, GetShoppingCartsBaseUrl());

    public static ApiProviderHelper GetShoppingCart(long shoppingCartID)
        => new(HttpMethod.Get, RouteType.Route, GetShoppingCartBaseUrl(shoppingCartID));

    public static ApiProviderHelper UpdateShoppingCart(long shoppingCartID)
        => new(HttpMethod.Patch, RouteType.Body, GetShoppingCartBaseUrl(shoppingCartID));

    #endregion Methods

}
