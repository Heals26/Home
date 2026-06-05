namespace Home.WebUI.DataAccess.RecipeIngredients.AddRecipeIngredient;

public class AddRecipeIngredientWebAppRequest
{

    #region Properties

    /// <summary>
    /// The name of the ingredient.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The quantity of the ingredient.
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// The ID of the recipe this ingredient belongs to.
    /// </summary>
    public long RecipeID { get; set; }

    /// <summary>
    /// The volume of the ingredient.
    /// </summary>
    public decimal? Volume { get; set; }

    /// <summary>
    /// The weight of the ingredient.
    /// </summary>
    public decimal? Weight { get; set; }

    #endregion Properties

}
