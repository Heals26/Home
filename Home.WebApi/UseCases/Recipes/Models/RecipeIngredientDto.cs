namespace Home.WebApi.UseCases.Recipes.Models;

public class RecipeIngredientDto
{

    #region Properties

    public long IngredientID { get; set; }
    public string Name { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Volume { get; set; }
    public decimal? Weight { get; set; }

    #endregion Properties

}
