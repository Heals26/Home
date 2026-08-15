namespace Home.Domain.Entities;

public class ShoppingListItem
{

    #region Properties

    public long ShoppingListItemID { get; set; }

    /// <summary>
    /// How much to buy, in <see cref="Unit"/>. Supersedes the three unitless columns below,
    /// which are kept only until the data move is proven and are no longer written to.
    /// </summary>
    public decimal? Amount { get; set; }

    public decimal? Cost { get; set; }
    public bool InBasket { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public long Sequence { get; set; }

    /// <summary>
    /// <see cref="Enumerations.MeasurementUnitSE"/> value. Null means an amount with no unit.
    /// </summary>
    public long? Unit { get; set; }

    public decimal? Volume { get; set; }
    public decimal? Weight { get; set; }

    public ShoppingList ShoppingList { get; set; } = null!;

    #endregion Properties

}
