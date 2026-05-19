namespace Home.WebUI.DataAccess.ShoppingCarts.GetShoppingCarts;

public class GetShoppingCartsWebAppResponse
{

    #region Properties
    public ICollection<GetShoppingCartDto> ShoppingCarts { get; set; }

    #endregion Properties

}

public class GetShoppingCartDto
{

    #region Properties

    public string CreatedBy { get; set; }
    public long ItemCount { get; set; }
    public string Name { get; set; }

    #endregion Properties

}