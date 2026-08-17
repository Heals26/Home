namespace Home.WebUI.DataAccess.ShoppingListItems.CreateShoppingListItem;

public class CreateShoppingListItemWebAppRequest
{

    #region Properties

    /// <summary>
    /// How much to buy, in <see cref="Unit"/>.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// The cost of the item.
    /// </summary>
    public decimal? Cost { get; set; }

    /// <summary>
    /// Whether the item is already in the basket.
    /// </summary>
    public bool InBasket { get; set; }

    /// <summary>
    /// The name of the item.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the shopping list this item belongs to.
    /// </summary>
    public long ShoppingListID { get; set; }

    /// <summary>
    /// The measurement the amount is in. Null is an amount with no unit.
    /// </summary>
    public long? Unit { get; set; }

    #endregion Properties

}
