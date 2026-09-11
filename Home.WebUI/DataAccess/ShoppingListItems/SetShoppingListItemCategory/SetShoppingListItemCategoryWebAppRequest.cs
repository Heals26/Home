namespace Home.WebUI.DataAccess.ShoppingListItems.SetShoppingListItemCategory;

public class SetShoppingListItemCategoryWebAppRequest
{

    #region Properties

    /// <summary>
    /// The aisle to file the item under, or null to take it out of the one it is in.
    /// </summary>
    public long? ShoppingCategoryID { get; set; }

    #endregion Properties

}
