namespace Home.WebUI.DataAccess.ShoppingLists.AddMealPlanToShoppingList;

public class AddMealPlanToShoppingListWebAppRequest
{

    #region Properties

    /// <summary>
    /// The first day of the planned window, inclusive.
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// Narrows the window to one meal of the day. Null takes every meal planned.
    /// </summary>
    public long? MealSlotID { get; set; }

    /// <summary>
    /// The last day of the planned window, inclusive.
    /// </summary>
    public DateTime ToDate { get; set; }

    #endregion Properties

}
