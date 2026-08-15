namespace Home.Domain.Entities;

public class Ingredient
{

    #region Properties

    public long IngredientID { get; set; }

    /// <summary>
    /// How much, in <see cref="Unit"/>. Supersedes the three unitless columns below, which are
    /// kept only until the data move is proven and are no longer written to.
    /// </summary>
    public decimal? Amount { get; set; }

    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }

    /// <summary>
    /// <see cref="Enumerations.MeasurementUnitSE"/> value. Null means an amount with no unit.
    /// </summary>
    public long? Unit { get; set; }

    public decimal? Volume { get; set; }
    public decimal? Weight { get; set; }

    public ICollection<IngredientNote> Notes { get; set; } = [];
    public ICollection<RecipeIngredient> Recipes { get; set; } = [];

    #endregion Properties

}
