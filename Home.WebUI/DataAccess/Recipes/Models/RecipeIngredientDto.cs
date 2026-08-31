namespace Home.WebUI.DataAccess.Recipes.Models;

public class RecipeIngredientDto
{

    #region Properties

    /// <summary>
    /// How much of the ingredient, in <see cref="Unit"/>.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// The ID of the ingredient.
    /// </summary>
    public long IngredientID { get; set; }

    /// <summary>
    /// The name of the ingredient.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Kept only so rows written before amounts carried a unit still read correctly.
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// Where it sits in this recipe's list — the order it is reached for while cooking.
    /// </summary>
    public long Sequence { get; set; }

    /// <summary>
    /// The measurement the amount is in.
    /// </summary>
    public long? Unit { get; set; }

    /// <summary>
    /// How the unit reads beside the amount, as the API resolved it.
    /// </summary>
    public string UnitAbbreviation { get; set; } = string.Empty;

    /// <summary>
    /// Kept only so rows written before amounts carried a unit still read correctly.
    /// </summary>
    public decimal? Volume { get; set; }

    /// <summary>
    /// Kept only so rows written before amounts carried a unit still read correctly.
    /// </summary>
    public decimal? Weight { get; set; }

    #endregion Properties

}
