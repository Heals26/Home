namespace Home.Domain.Entities;

/// <summary>
/// What the household knows about something it buys, kept by name so it outlives the list line.
/// Clearing the ticked items deletes the lines; the aisle an item goes in and what it cost are
/// remembered here instead.
/// </summary>
public class ShoppingItemMemory
{

    #region Properties

    public long ShoppingItemMemoryID { get; set; }

    public Household Household { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The name trimmed and lower-cased, so "Milk" and "milk " are one item on every database, not
    /// only on one whose collation happens to ignore case.
    /// </summary>
    public string NameKey { get; set; } = string.Empty;

    public ICollection<ShoppingItemPrice> Prices { get; set; } = [];

    public ShoppingCategory? ShoppingCategory { get; set; }

    #endregion Properties

}
