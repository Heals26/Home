using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.ShoppingListItems.UpdateShoppingListItem;

/// <summary>
/// Omit a property to leave it alone, so ticking an item off can't clobber its name or cost.
/// </summary>
public class UpdateShoppingListItemWebAppRequest
{

    #region Properties

    /// <summary>
    /// The cost of one of the item.
    /// </summary>
    public PropertyChangeTracker<decimal?> Cost { get; set; }

    /// <summary>
    /// Whether the item is in the trolley.
    /// </summary>
    public PropertyChangeTracker<bool> InBasket { get; set; }

    /// <summary>
    /// The item's name.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    /// <summary>
    /// How many of the item to buy.
    /// </summary>
    public PropertyChangeTracker<decimal?> Quantity { get; set; }

    /// <summary>
    /// Display order within the list.
    /// </summary>
    public PropertyChangeTracker<long> Sequence { get; set; }

    /// <summary>
    /// The ID of the item — mirrored into the body alongside the route.
    /// </summary>
    public long ShoppingListItemID { get; set; }

    /// <summary>
    /// The item's volume in millilitres.
    /// </summary>
    public PropertyChangeTracker<decimal?> Volume { get; set; }

    /// <summary>
    /// The item's weight in grams.
    /// </summary>
    public PropertyChangeTracker<decimal?> Weight { get; set; }

    #endregion Properties

}
