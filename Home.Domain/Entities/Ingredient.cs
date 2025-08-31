namespace Home.Domain.Entities;

public class Ingredient
{

    #region Properties

    public long IngredientID { get; set; }
    public string Name { get; set; }
    public decimal Quantity { get; set; }
    public decimal Volumne { get; set; }
    public decimal Weight { get; set; }

    public ICollection<RecipeIngredient> Recipes { get; set; }

    #endregion Properties

}
