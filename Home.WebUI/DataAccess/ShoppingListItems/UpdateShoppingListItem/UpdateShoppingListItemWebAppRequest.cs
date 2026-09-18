using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.ShoppingListItems.UpdateShoppingListItem;

/// <summary>
/// The sheet behind a line. Omit a property to leave it alone, so saving one field cannot clobber
/// another that someone changed on their own device. Ticking and moving have their own requests.
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
    /// The item's name.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    /// <summary>
    /// What the name does not say. Send it empty to clear it.
    /// </summary>
    public PropertyChangeTracker<string?> Note { get; set; }

    /// <summary>
    /// The ID of the item, mirrored into the body alongside the route.
    /// </summary>
    public long ShoppingListItemID { get; set; }

    /// <summary>
    /// The measurement the amount is in. Null is an amount with no unit.
    /// </summary>
    public PropertyChangeTracker<long?> Unit { get; set; }

    #endregion Properties

}
