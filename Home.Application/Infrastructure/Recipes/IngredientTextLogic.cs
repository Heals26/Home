using Home.Domain.Enumerations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Home.Application.Infrastructure.Recipes;

/// <summary>
/// Reads "2 cups plain flour" as two cups of plain flour. A site writes its ingredients as one line
/// of text each, and a line that keeps the amount inside its name cannot be put on a shopping list
/// as anything but words: adding it gives a line called "2 cups plain flour" with no amount at all.
/// <para>
/// The web app reads the quick add box the same way in <c>ShoppingListItemLogic</c>. The two are
/// deliberate twins, because the front end is a separate app rather than a shared project, so a way
/// of writing a unit taught to one belongs in the other.
/// </para>
/// </summary>
public static partial class IngredientTextLogic
{

    #region Fields

    /// <summary>
    /// Every way a unit gets written, mapped to the value the app stores. The forms the enumeration
    /// itself carries are added first, and these are what English does to them in a recipe. A
    /// recognised entry with no value is a bare multiplier: "2 x eggs" is two eggs, not two of some
    /// measurement.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, long?> s_Units = BuildUnits(new Dictionary<string, long?>(StringComparer.OrdinalIgnoreCase)
    {
        ["gram"] = 2, ["grams"] = 2,
        ["kgs"] = 3, ["kilo"] = 3, ["kilos"] = 3, ["kilogram"] = 3, ["kilograms"] = 3,
        ["mls"] = 4, ["millilitre"] = 4, ["millilitres"] = 4,
        ["lt"] = 5, ["litre"] = 5, ["litres"] = 5,
        ["tsps"] = 6, ["teaspoon"] = 6, ["teaspoons"] = 6,
        ["tbsps"] = 7, ["tablespoon"] = 7, ["tablespoons"] = 7,
        ["can"] = 13, ["cans"] = 13,
        ["pack"] = 14, ["packs"] = 14, ["pkt"] = 14, ["pk"] = 14,
        ["stick"] = 17, ["sticks"] = 17,
        ["x"] = null
    });

    #endregion Fields

    #region Methods

    private static IReadOnlyDictionary<string, long?> BuildUnits(Dictionary<string, long?> written)
    {
        foreach (var _Unit in BaseEnumeration.GetAll<MeasurementUnitSE>().Where(u => u.Abbreviation.Length > 0))
        {
            written[_Unit.Abbreviation] = _Unit.Value;
            written[_Unit.SingularAbbreviation] = _Unit.Value;
            written[_Unit.Name] = _Unit.Value;
        }

        return written;
    }

    /// <summary>
    /// Only the first letter, so "BBQ sauce" and "pak choi" both survive.
    /// </summary>
    private static string Capitalise(string text)
        => char.IsLower(text[0]) ? string.Concat(char.ToUpperInvariant(text[0]), text[1..]) : text;

    private static string CollapseWhitespace(string? text)
        => string.IsNullOrWhiteSpace(text) ? string.Empty : WhitespacePattern().Replace(text.Trim(), " ");

    /// <summary>
    /// Reads a leading amount and unit off the text and treats the rest as the name. Anything it
    /// cannot make sense of stays in the name untouched, so an odd line is never lost.
    /// </summary>
    public static ParsedIngredient Parse(string? text)
    {
        var _Text = CollapseWhitespace(text);

        if (_Text.Length == 0)
            return new(null, string.Empty, null);

        var _Match = AmountPattern().Match(_Text);

        if (!_Match.Success)
            return new(null, Capitalise(_Text), null);

        var _Amount = ReadAmount(_Match.Groups["whole"].Value, _Match.Groups["numerator"].Value, _Match.Groups["denominator"].Value);

        if (_Amount == null)
            return new(null, Capitalise(_Text), null);

        long? _Unit = null;
        var _Remainder = _Match.Groups["remainder"].Value.TrimStart();

        // "500g mince" runs the unit straight onto the number, so the word touching it is tried as a
        // unit before the text is split on spaces.
        var _Attached = _Match.Groups["attached"].Value;

        if (_Attached.Length > 0)
        {
            if (!s_Units.TryGetValue(_Attached, out _Unit))
                return new(null, Capitalise(_Text), null);
        }
        else
        {
            var _Space = _Remainder.IndexOf(' ');
            var _FirstWord = _Space < 0 ? _Remainder : _Remainder[.._Space];

            if (_FirstWord.Length > 0 && s_Units.TryGetValue(_FirstWord, out var _NamedUnit))
            {
                _Unit = _NamedUnit;
                _Remainder = _Space < 0 ? string.Empty : _Remainder[(_Space + 1)..];
            }
        }

        // An amount with nothing left to buy is not an amount, it is the name of the thing.
        return _Remainder.Length == 0
            ? new(null, Capitalise(_Text), null)
            : new(_Amount, Capitalise(_Remainder), _Unit);
    }

    private static decimal? ReadAmount(string whole, string numerator, string denominator)
    {
        if (numerator.Length > 0)
        {
            return decimal.TryParse(numerator, NumberStyles.Number, CultureInfo.InvariantCulture, out var _Numerator)
                && decimal.TryParse(denominator, NumberStyles.Number, CultureInfo.InvariantCulture, out var _Denominator)
                && _Denominator != 0
                    ? _Numerator / _Denominator
                    : null;
        }

        return decimal.TryParse(whole, NumberStyles.Number, CultureInfo.InvariantCulture, out var _Whole) ? _Whole : null;
    }

    [GeneratedRegex(@"^(?:(?<numerator>\d+)\s*/\s*(?<denominator>\d+)|(?<whole>\d+(?:\.\d+)?))(?<attached>[a-zA-Z]*)(?<remainder>\s.*)?$")]
    private static partial Regex AmountPattern();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespacePattern();

    #endregion Methods

}
