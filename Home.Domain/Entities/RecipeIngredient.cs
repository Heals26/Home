namespace Home.Domain.Entities;

public class RecipeIngredient
{

    #region Properties

    public long IngredientID { get; set; }
    public long RecipeID { get; set; }

    public Ingredient Ingredient { get; set; } = null!;
    public Recipe Recipe { get; set; } = null!;

    #endregion Properties

}
