namespace Home.WebUI.DataAccess.Recipes.Models;

/// <summary>
/// A mirror of the API's measurement enumeration, kept here because the web app does not
/// reference the domain. The values must stay in step with Home.Domain's MeasurementUnitSE.
/// </summary>
public static class MeasurementUnits
{

    #region Fields

    public static readonly IReadOnlyList<MeasurementUnitOption> All =
    [
        new("", "Pieces", 1),
        new("g", "Grams", 2),
        new("kg", "Kilograms", 3),
        new("ml", "Millilitres", 4),
        new("L", "Litres", 5),
        new("tsp", "Teaspoons", 6),
        new("tbsp", "Tablespoons", 7),
        new("cups", "Cups", 8),
        new("pinch", "Pinch", 9),
        new("bunch", "Bunch", 10),
        new("slices", "Slices", 11),
        new("cloves", "Cloves", 12),
        new("tins", "Tins", 13),
        new("packets", "Packets", 14),
    ];

    #endregion Fields

    #region Methods

    public static string GetAbbreviation(long? unit)
        => All.FirstOrDefault(u => u.Value == unit)?.Abbreviation ?? string.Empty;

    #endregion Methods

}
