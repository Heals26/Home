namespace Home.WebApi.UseCases.ShoppingCartItems.CreateShoppingCartItem;

public class CreateShoppingCartItemApiRequest
{

    #region Properties

    public int Cost { get; set; }
    public bool InBasket { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }

    #endregion Properties

}
