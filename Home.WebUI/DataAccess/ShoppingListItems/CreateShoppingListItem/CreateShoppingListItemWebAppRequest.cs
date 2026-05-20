namespace Home.WebUI.DataAccess.ShoppingListItems.CreateShoppingListItem;

public class CreateShoppingListItemWebAppRequest
{

    #region Properties

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
    /// The quantity of the item.
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// The ID of the shopping list this item belongs to.
    /// </summary>
    public long ShoppingListID { get; set; }

    /// <summary>
    /// The volume of the item.
    /// </summary>
    public decimal? Volume { get; set; }

    /// <summary>
    /// The weight of the item.
    /// </summary>
    public decimal? Weight { get; set; }

    #endregion Properties

}
