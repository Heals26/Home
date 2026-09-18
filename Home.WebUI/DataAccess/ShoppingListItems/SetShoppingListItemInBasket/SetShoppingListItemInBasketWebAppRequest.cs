namespace Home.WebUI.DataAccess.ShoppingListItems.SetShoppingListItemInBasket;

/// <summary>
/// Ticks a line into the trolley, or takes it back out.
/// </summary>
public class SetShoppingListItemInBasketWebAppRequest
{

    #region Properties

    /// <summary>
    /// Whether the item is in the trolley. During a shop this is what records what it cost, and
    /// taking it back out takes that purchase back.
    /// </summary>
    public bool InBasket { get; set; }

    #endregion Properties

}
