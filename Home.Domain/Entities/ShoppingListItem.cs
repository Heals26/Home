namespace Home.Domain.Entities;

public class ShoppingListItem
{

    #region Properties

    public long ShoppingListItemID { get; set; }

    /// <summary>
    /// How much to buy, in <see cref="Unit"/>. Replaced three unitless columns (Quantity, Volume
    /// and Weight), which were dropped on 4 Sep 2026 once every row had moved across.
    /// </summary>
    public decimal? Amount { get; set; }

    public decimal? Cost { get; set; }
    public bool InBasket { get; set; }
    public string Name { get; set; } = string.Empty;
    public long Sequence { get; set; }

    /// <summary>
    /// <see cref="Enumerations.MeasurementUnitSE"/> value. Null means an amount with no unit.
    /// </summary>
    public long? Unit { get; set; }


    public ShoppingList ShoppingList { get; set; } = null!;

    #endregion Properties

}
