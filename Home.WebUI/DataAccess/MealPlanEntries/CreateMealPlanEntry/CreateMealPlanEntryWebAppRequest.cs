namespace Home.WebUI.DataAccess.MealPlanEntries.CreateMealPlanEntry;

public class CreateMealPlanEntryWebAppRequest
{

    #region Properties

    /// <summary>
    /// The local calendar day to plan the recipe for.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The ID of the recipe to plan.
    /// </summary>
    public long RecipeID { get; set; }

    #endregion Properties

}
