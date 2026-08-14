namespace Home.WebUI.DataAccess.MealPlanEntries.Models;

public class MealPlanEntryDto
{

    #region Properties

    /// <summary>
    /// The local calendar day the meal is planned for; only the date is meaningful.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The ID of the meal plan entry.
    /// </summary>
    public long MealPlanEntryID { get; set; }

    /// <summary>
    /// The ID of the planned recipe.
    /// </summary>
    public long RecipeID { get; set; }

    /// <summary>
    /// The name of the planned recipe.
    /// </summary>
    public string RecipeName { get; set; } = string.Empty;

    #endregion Properties

}
