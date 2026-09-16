namespace Home.Domain.Entities;

/// <summary>
/// One time the household bought something, and what it paid. Written when a priced line is ticked
/// during a shop, or priced afterwards while it is still ticked.
/// </summary>
public class ShoppingItemPrice
{

    #region Properties

    public long ShoppingItemPriceID { get; set; }

    public decimal? Amount { get; set; }

    public DateTime BoughtOnUTC { get; set; }

    /// <summary>
    /// The line price, as on the list: what was paid for <see cref="Amount"/>, not per unit.
    /// </summary>
    public decimal Cost { get; set; }

    public ShoppingItemMemory Memory { get; set; } = null!;

    /// <summary>
    /// The list line that recorded this. With <see cref="ShoppingTripID"/> it names one purchase, so
    /// correcting a price corrects the record rather than adding a second. Not a foreign key, because
    /// clearing the ticked items deletes the line and the record has to stay.
    /// </summary>
    public long ShoppingListItemID { get; set; }

    /// <summary>
    /// The trip the purchase was made on. Not a foreign key either: a list's trips go with the list,
    /// and what was paid has to outlive both. Null for purchases remembered before trips existed.
    /// </summary>
    public long? ShoppingTripID { get; set; }

    public long? Unit { get; set; }

    #endregion Properties

}
