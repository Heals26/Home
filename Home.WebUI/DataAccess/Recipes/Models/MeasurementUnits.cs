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
        new("", "Pieces", 1, ""),
        new("g", "Grams", 2, "g"),
        new("kg", "Kilograms", 3, "kg"),
        new("ml", "Millilitres", 4, "ml"),
        new("L", "Litres", 5, "L"),
        new("tsp", "Teaspoons", 6, "tsp"),
        new("tbsp", "Tablespoons", 7, "tbsp"),
        new("cups", "Cups", 8, "cup"),
        new("pinches", "Pinch", 9, "pinch"),
        new("bunches", "Bunch", 10, "bunch"),
        new("slices", "Slices", 11, "slice"),
        new("cloves", "Cloves", 12, "clove"),
        new("tins", "Tins", 13, "tin"),
        new("packets", "Packets", 14, "packet"),
        new("jars", "Jars", 15, "jar"),
        new("leaves", "Leaves", 16, "leaf"),
        new("stalks", "Stalks", 17, "stalk"),
        new("dashes", "Dashes", 18, "dash"),
    ];

    #endregion Fields

    #region Methods

    /// <summary>
    /// The form that suits the amount beside it: exactly one reads singular, anything else —
    /// including a half and an amount nobody has given yet — reads plural.
    /// </summary>
    public static string GetAbbreviation(long? unit, decimal? amount)
    {
        var _Unit = All.FirstOrDefault(u => u.Value == unit);

        if (_Unit == null)
            return string.Empty;

        return amount == 1
            ? _Unit.SingularAbbreviation
            : _Unit.Abbreviation;
    }

    #endregion Methods

}
