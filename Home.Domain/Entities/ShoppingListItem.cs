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

    /// <summary>
    /// What whoever is at the shop needs to know that the name does not say: a brand, a size, which
    /// aisle, who it is for. Null when there is nothing to add.
    /// <para>
    /// A column rather than a row in <see cref="Note"/> like a recipe or ingredient note, because
    /// this is a property of one line on one list and not a document. Clearing the ticked items
    /// deletes the line and the note goes with it, which is the wanted behaviour and is free here.
    /// </para>
    /// </summary>
    public string? Note { get; set; }

    public long Sequence { get; set; }

    /// <summary>
    /// <see cref="Enumerations.MeasurementUnitSE"/> value. Null means an amount with no unit.
    /// </summary>
    public long? Unit { get; set; }


    public ShoppingList ShoppingList { get; set; } = null!;

    #endregion Properties

}
