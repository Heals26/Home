namespace Home.Domain.Entities;

public class ShoppingListItem
{

    #region - - - - - - Properties - - - - - -

    public long ShoppingListItemID { get; set; }
    public int Cost { get; set; }
    public bool InBasket { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public long Sequence { get; set; }

    public ShoppingList ShoppingList { get; set; }

    #endregion Properties

}
