using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.ShoppingListItems.UpdateShoppingListItem;

/// <summary>
/// Omit a property to leave it alone, so ticking an item off can't clobber its name or cost.
/// </summary>
public class UpdateShoppingListItemWebAppRequest
{

    #region Properties

    /// <summary>
    /// How much to buy, in <see cref="Unit"/>.
    /// </summary>
    public PropertyChangeTracker<decimal?> Amount { get; set; }

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
    /// Display order within the list.
    /// </summary>
    public PropertyChangeTracker<long> Sequence { get; set; }

    /// <summary>
    /// The ID of the item — mirrored into the body alongside the route.
    /// </summary>
    public long ShoppingListItemID { get; set; }

    /// <summary>
    /// The measurement the amount is in. Null is an amount with no unit.
    /// </summary>
    public PropertyChangeTracker<long?> Unit { get; set; }

    #endregion Properties

}
