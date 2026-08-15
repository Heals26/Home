namespace Home.WebUI.DataAccess.Recipes.Models;

public class RecipeMealSlotDto
{

    #region Properties

    /// <summary>
    /// The ID of the meal this recipe suits.
    /// </summary>
    public long MealSlotID { get; set; }

    /// <summary>
    /// What the household calls the meal.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display order through the day.
    /// </summary>
    public int Sequence { get; set; }

    #endregion Properties

}
