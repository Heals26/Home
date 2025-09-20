namespace Home.WebApi.UseCases.ShoppingCarts.Models;

public class ShoppingCartItemDto
{

    #region Properties

    public int Cost { get; set; }
    public bool InBasket { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public long Sequence { get; set; }
    public long ShoppingCartItemID { get; set; }

    #endregion Properties

}
