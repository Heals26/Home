namespace Home.Domain.Entities;

/// <summary>
/// One time the household bought something, and what it paid. Written when the item goes into the
/// trolley with a price on it.
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
    /// The list line that recorded this, so ticking the same line again in the same shop corrects
    /// the record rather than adding a second. Not a foreign key, because clearing the ticked items
    /// deletes the line and the record has to stay.
    /// </summary>
    public long ShoppingListItemID { get; set; }

    public long? Unit { get; set; }

    #endregion Properties

}
