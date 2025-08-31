namespace Home.Domain.Entities;

public class RecipeIngredient
{

    #region Properties

    public long IngredientID { get; set; }
    public long RecipeID { get; set; }

    public Ingredient Ingredient { get; set; }
    public Recipe Recipe { get; set; }

    #endregion Properties

}
