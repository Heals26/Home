using Home.Application.Infrastructure.Recipes;

namespace Home.WebApi.UseCases.Recipes.Models;

public class RecipeIngredientDto
{

    #region Properties

    /// <summary>
    /// How much, in <see cref="Unit"/>.
    /// </summary>
    public decimal? Amount { get; set; }

    public long IngredientID { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// Where it sits in this recipe's list — the order it is reached for while cooking.
    /// </summary>
    public long Sequence { get; set; }

    /// <summary>
    /// Kept only so rows written before amounts carried a unit still read correctly.
    /// </summary>
    public decimal? Quantity { get; set; }

    public long? Unit { get; set; }

    /// <summary>
    /// How the unit reads beside the amount, resolved here so every screen says the same thing.
    /// </summary>
    public string UnitAbbreviation
        => MeasurementUnitLogic.GetAbbreviation(this.Unit, this.Amount);

    public decimal? Volume { get; set; }
    public decimal? Weight { get; set; }

    #endregion Properties

}
