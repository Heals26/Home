using Home.WebUI.DataAccess.ShoppingCarts.Models;

namespace Home.WebUI.DataAccess.ShoppingCarts.GetShoppingCart;

public class GetShoppingCartWebAppResponse
{

    #region Properties

    /// <summary>
    /// The name of the shopping cart
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The items in the shopping cart
    /// </summary>
    public List<ShoppingCartItemDto> Items { get; set; }

    #endregion Properties

}
