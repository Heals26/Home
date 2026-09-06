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

    /// <summary>
    /// What to show on the day: the recipe's name, or the title when the entry is an occasion
    /// rather than a dish. Always populated, so nothing reading this needs to know which it is.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The recipe behind the entry, or null when it is an occasion. This is what says whether the
    /// name is something you can open.
    /// </summary>
    public long? RecipeID { get; set; }

    #endregion Properties

}
