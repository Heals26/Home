namespace Home.WebUI.DataAccess.ShoppingLists.Models;

public class ShoppingListItemDto
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
    /// Whether the item has been placed in the basket.
    /// </summary>
    public bool InBasket { get; set; }

    /// <summary>
    /// The name of the item.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Kept only so rows written before amounts carried a unit still read correctly.
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
    /// The measurement the amount is in.
    /// </summary>
    public long? Unit { get; set; }

    /// <summary>
    /// How the unit reads beside the amount, as the API resolved it.
    /// </summary>
    public string UnitAbbreviation { get; set; } = string.Empty;

    /// <summary>
    /// Kept only so rows written before amounts carried a unit still read correctly.
    /// </summary>
    public decimal? Volume { get; set; }

    /// <summary>
    /// Kept only so rows written before amounts carried a unit still read correctly.
    /// </summary>
    public decimal? Weight { get; set; }

    #endregion Properties

}
