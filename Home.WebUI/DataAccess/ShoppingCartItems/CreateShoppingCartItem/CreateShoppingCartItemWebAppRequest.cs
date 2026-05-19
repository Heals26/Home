namespace Home.WebUI.DataAccess.ShoppingCartItems.CreateShoppingCartItem;

public class CreateShoppingCartItemWebAppRequest
{

    #region Properties

    public int Cost { get; set; }
    public bool InBasket { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }

    #endregion Properties

}
