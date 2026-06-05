namespace Home.WebUI.DataAccess.Recipes.Models;

public class RecipeIngredientDto
{

    #region Properties

    /// <summary>
    /// The ID of the ingredient.
    /// </summary>
    public long IngredientID { get; set; }

    /// <summary>
    /// The name of the ingredient.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The quantity of the ingredient.
    /// </summary>
    public decimal? Quantity { get; set; }

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
