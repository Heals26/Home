namespace Home.WebApi.UseCases.ShoppingCarts.GetShoppingCarts;

public class GetShoppingCartsApiResponse
{

    #region Properties

    /// <summary>
    /// A collection of shopping carts
    /// </summary>
    public ICollection<GetShoppingCartDto> ShoppingCarts { get; set; }

    #endregion Properties

}

public class GetShoppingCartDto
{

    #region Properties

    /// <summary>
    /// Who created the Shopping Cart
    /// </summary>
    public string CreatedBy { get; set; }

    /// <summary>
    /// The number of items in the shopping cart
    /// </summary>
    public long ItemCount { get; set; }

    /// <summary>
    /// The name of the shopping cart
    /// </summary>
    public string Name { get; set; }

    #endregion Properties

}
