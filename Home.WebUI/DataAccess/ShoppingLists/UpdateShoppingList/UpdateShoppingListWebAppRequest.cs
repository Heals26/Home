using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.ShoppingLists.UpdateShoppingList;

public class UpdateShoppingListWebAppRequest
{

    #region Properties

    /// <summary>
    /// Whether the list reads by aisle rather than in the household's own order.
    /// </summary>
    public PropertyChangeTracker<bool> GroupByAisle { get; set; }

    /// <summary>
    /// Whether the list is put away out of the picker. Archiving keeps everything it holds.
    /// </summary>
    public PropertyChangeTracker<bool> IsArchived { get; set; }

    /// <summary>
    /// What the list is called.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    #endregion Properties

}
