using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.ShoppingLists.UpdateShoppingList;

public class UpdateShoppingListWebAppRequest
{

    #region Properties

    /// <summary>
    /// The name of the shopping list.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    #endregion Properties

}
