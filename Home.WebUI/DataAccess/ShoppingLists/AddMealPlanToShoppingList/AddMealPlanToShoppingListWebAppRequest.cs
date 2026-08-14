namespace Home.WebUI.DataAccess.ShoppingLists.AddMealPlanToShoppingList;

public class AddMealPlanToShoppingListWebAppRequest
{

    #region Properties

    /// <summary>
    /// The first day of the planned window, inclusive.
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// The last day of the planned window, inclusive.
    /// </summary>
    public DateTime ToDate { get; set; }

    #endregion Properties

}
