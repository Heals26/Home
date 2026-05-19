namespace Home.Domain.Entities;

public class ShoppingListItem
{

    #region Properties

    public long ShoppingListItemID { get; set; }
    public decimal? Cost { get; set; }
    public bool InBasket { get; set; }
    public string Name { get; set; }
    public decimal? Quantity { get; set; }
    public long Sequence { get; set; }
    public decimal? Volume { get; set; }
    public decimal? Weight { get; set; }

    public ShoppingList ShoppingList { get; set; }

    #endregion Properties

}
