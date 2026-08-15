namespace Home.WebApi.UseCases.MealPlanEntries.Models;

public class MealPlanEntryDto
{

    #region Properties

    public DateTime Date { get; set; }
    public long MealPlanEntryID { get; set; }

    /// <summary>
    /// Null on entries planned before the household defined its meals.
    /// </summary>
    public long? MealSlotID { get; set; }

    public string MealSlotName { get; set; } = string.Empty;
    public long RecipeID { get; set; }
    public string RecipeName { get; set; } = string.Empty;

    #endregion Properties

}
