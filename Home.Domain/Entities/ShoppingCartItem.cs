namespace Home.Domain.Entities;

public class ShoppingCartItem
{

    #region Properties

    public long ShoppingCartItemID { get; set; }
    public int Cost { get; set; }
    public bool InBasket { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public long Sequence { get; set; }

    public ShoppingCart ShoppingCart { get; set; }

    #endregion Properties

}
