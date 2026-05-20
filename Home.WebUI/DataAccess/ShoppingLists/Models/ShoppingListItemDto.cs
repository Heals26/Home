namespace Home.WebUI.DataAccess.ShoppingLists.Models;

public class ShoppingListItemDto
{

    #region Properties

    /// <summary>
    /// The cost of the item.
    /// </summary>
    public decimal? Cost { get; set; }

    /// <summary>
    /// Whether the item has been placed in the basket.
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
    /// The display sequence of the item.
    /// </summary>
    public long Sequence { get; set; }

    /// <summary>
    /// The ID of the shopping list item.
    /// </summary>
    public long ShoppingListItemID { get; set; }

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
