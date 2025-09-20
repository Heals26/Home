using Home.WebApi.UseCases.ShoppingCarts.Models;

namespace Home.WebApi.UseCases.ShoppingCarts.GetShoppingCart;

public class GetShoppingCartApiResponse
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
