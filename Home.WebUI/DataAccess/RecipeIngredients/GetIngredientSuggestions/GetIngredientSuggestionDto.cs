namespace Home.WebUI.DataAccess.RecipeIngredients.GetIngredientSuggestions;

public class GetIngredientSuggestionDto
{

    #region Properties

    /// <summary>
    /// The amount it was last written with.
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// The ingredient as it was last written.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// How many of the household's recipes use it.
    /// </summary>
    public long TimesUsed { get; set; }

    /// <summary>
    /// The measurement the amount was last in.
    /// </summary>
    public long? Unit { get; set; }

    /// <summary>
    /// How the unit reads beside the amount, as the API resolved it.
    /// </summary>
    public string UnitAbbreviation { get; set; } = string.Empty;

    #endregion Properties

}
