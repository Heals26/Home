namespace Home.Domain.Entities;

public class RecipeIngredient
{

    #region Properties

    public long IngredientID { get; set; }
    public long RecipeID { get; set; }

    public Ingredient Ingredient { get; set; } = null!;
    public Recipe Recipe { get; set; } = null!;

    /// <summary>
    /// Where the ingredient sits in this recipe's list — the order it is reached for while
    /// cooking, which is how a cookbook sets them. It lives on the join rather than on the
    /// ingredient because the position belongs to one recipe, not to the thing itself.
    /// </summary>
    public long Sequence { get; set; }

    #endregion Properties

}
