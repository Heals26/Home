namespace Home.WebUI.DataAccess.RecipeIngredients.AddRecipeIngredient;

public class AddRecipeIngredientWebAppRequest
{

    #region Properties

    /// <summary>
    /// How much of the ingredient, in <see cref="Unit"/>.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// The name of the ingredient.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the recipe this ingredient belongs to.
    /// </summary>
    public long RecipeID { get; set; }

    /// <summary>
    /// The measurement the amount is in. Null means a plain count.
    /// </summary>
    public long? Unit { get; set; }

    #endregion Properties

}
