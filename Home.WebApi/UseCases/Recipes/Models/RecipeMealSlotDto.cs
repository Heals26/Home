namespace Home.WebApi.UseCases.Recipes.Models;

public class RecipeMealSlotDto
{

    #region Properties

    public long MealSlotID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Sequence { get; set; }

    #endregion Properties

}
