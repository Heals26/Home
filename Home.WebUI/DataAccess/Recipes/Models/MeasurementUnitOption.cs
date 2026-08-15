namespace Home.WebUI.DataAccess.Recipes.Models;

/// <summary>
/// One entry in the cooking measurement dropdown.
/// </summary>
/// <param name="Abbreviation">How the unit reads beside an amount, e.g. "tbsp".</param>
/// <param name="Name">How the unit reads in the dropdown, e.g. "Tablespoons".</param>
/// <param name="Value">The stored value the API exchanges.</param>
public record MeasurementUnitOption(string Abbreviation, string Name, long Value);
