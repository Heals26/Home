namespace Home.WebApi.UseCases.MealPlanEntries.Models;

public class MealPlanEntryDto
{

    #region Properties

    public DateTime Date { get; set; }
    public long MealPlanEntryID { get; set; }
    public long RecipeID { get; set; }
    public string RecipeName { get; set; } = string.Empty;

    #endregion Properties

}
