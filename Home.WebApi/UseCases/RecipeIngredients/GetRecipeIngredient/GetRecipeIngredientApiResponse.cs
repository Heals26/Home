using Home.Application.Infrastructure.Recipes;
using Home.WebApi.UseCases.Ingredients.Models;

namespace Home.WebApi.UseCases.RecipeIngredients.GetRecipeIngredient;

public class GetRecipeIngredientApiResponse
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
    public string Name { get; set; }

    /// <summary>
    /// Notes that have been added to this ingredient.
    /// </summary>
    public List<IngredientNoteDto> Notes { get; set; }

    /// <summary>
    /// Kept only so rows written before amounts carried a unit still read correctly.
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// The measurement the amount is in.
    /// </summary>
    public long? Unit { get; set; }

    /// <summary>
    /// How the unit reads beside the amount.
    /// </summary>
    public string UnitAbbreviation
        => MeasurementUnitLogic.GetAbbreviation(this.Unit);

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
