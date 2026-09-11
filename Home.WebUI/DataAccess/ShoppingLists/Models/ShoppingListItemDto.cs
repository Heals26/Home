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
    /// For a line with no price, roughly what it will cost, from what the household paid last time.
    /// </summary>
    public decimal? EstimatedCost { get; set; }

    /// <summary>
    /// Whether the item has been placed in the basket.
    /// </summary>
    public bool InBasket { get; set; }

    /// <summary>
    /// The line's price is more than a tenth over what it usually costs.
    /// </summary>
    public bool IsDearerThanUsual { get; set; }

    /// <summary>
    /// The name of the item.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// What the name does not say: a brand, a size, which aisle, who it is for. Empty when the
    /// line carries nothing extra.
    /// </summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// The display sequence of the item.
    /// </summary>
    public long Sequence { get; set; }

    /// <summary>
    /// The aisle the item is filed under, the same on every list. Null until someone picks one.
    /// </summary>
    public long? ShoppingCategoryID { get; set; }

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
    /// What the line usually costs at this amount, worked out per unit from past purchases.
    /// </summary>
    public decimal? UsualCost { get; set; }

    #endregion Properties

}
