namespace Home.Domain.Entities;

public class Ingredient
{

    #region Properties

    public long IngredientID { get; set; }

    /// <summary>
    /// How much, in <see cref="Unit"/>. Replaced three unitless columns (Quantity, Volume and
    /// Weight), which were dropped on 4 Sep 2026 once every row had moved across.
    /// </summary>
    public decimal? Amount { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// <see cref="Enumerations.MeasurementUnitSE"/> value. Null means an amount with no unit.
    /// </summary>
    public long? Unit { get; set; }


    public ICollection<IngredientNote> Notes { get; set; } = [];
    public ICollection<RecipeIngredient> Recipes { get; set; } = [];

    #endregion Properties

}
