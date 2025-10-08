namespace Home.WebUI.Infrastructure.UriProvider;

public static class ShoppingCartUriProvider
{

    #region Methods

    public static string GetShoppingCartsUri()
        => $"/shopping-carts";

    public static string GetShoppingCartUri(long shoppingCartID)
        => $"{GetShoppingCartsUri}/{shoppingCartID}";

    #endregion Methods

}
