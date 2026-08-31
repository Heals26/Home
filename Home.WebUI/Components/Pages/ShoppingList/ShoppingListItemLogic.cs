using System.Globalization;
using System.Text.RegularExpressions;
using Home.WebUI.DataAccess.Recipes.Models;
using Home.WebUI.DataAccess.ShoppingLists.Models;

namespace Home.WebUI.Components.Pages.ShoppingList;

/// <summary>
/// The one place that turns what someone typed into an item, and an item back into words. Adding
/// to a list is the thing this app is asked to do most often, so it accepts how people actually
/// write a list — "2 kg potatoes", "500g mince", "1/2 cup rice" — rather than making them fill in
/// three boxes.
/// </summary>
public static partial class ShoppingListItemLogic
{

    #region Fields

    /// <summary>
    /// Every way a unit gets written on a shopping list, mapped to the value the API stores. A
    /// recognised entry with no value is a bare multiplier — "2 x eggs" is two eggs, not two of
    /// some measurement.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, long?> c_Units = new Dictionary<string, long?>(StringComparer.OrdinalIgnoreCase)
    {
        ["g"] = 2, ["gram"] = 2, ["grams"] = 2,
        ["kg"] = 3, ["kgs"] = 3, ["kilo"] = 3, ["kilos"] = 3, ["kilogram"] = 3, ["kilograms"] = 3,
        ["ml"] = 4, ["mls"] = 4, ["millilitre"] = 4, ["millilitres"] = 4,
        ["l"] = 5, ["lt"] = 5, ["litre"] = 5, ["litres"] = 5,
        ["tsp"] = 6, ["tsps"] = 6, ["teaspoon"] = 6, ["teaspoons"] = 6,
        ["tbsp"] = 7, ["tbsps"] = 7, ["tablespoon"] = 7, ["tablespoons"] = 7,
        ["cup"] = 8, ["cups"] = 8,
        ["pinch"] = 9, ["pinches"] = 9,
        ["bunch"] = 10, ["bunches"] = 10,
        ["slice"] = 11, ["slices"] = 11,
        ["clove"] = 12, ["cloves"] = 12,
        ["tin"] = 13, ["tins"] = 13, ["can"] = 13, ["cans"] = 13,
        ["packet"] = 14, ["packets"] = 14, ["pack"] = 14, ["packs"] = 14, ["pkt"] = 14, ["pk"] = 14,
        ["jar"] = 15, ["jars"] = 15,
        ["leaf"] = 16, ["leaves"] = 16,
        ["stalk"] = 17, ["stalks"] = 17, ["stick"] = 17, ["sticks"] = 17,
        ["dash"] = 18, ["dashes"] = 18,
        ["x"] = null
    };

    #endregion Fields

    #region Methods

    /// <summary>
    /// Amounts written before units existed only had a bare quantity, a volume in millilitres or
    /// a weight in grams, so those are still read when there is no amount to show.
    /// </summary>
    public static string DescribeAmount(ShoppingListItemDto item)
    {
        if (item.Amount != null)
            return DescribeAmount(item.Amount, item.Unit, item.UnitAbbreviation);

        List<string> _Legacy = [];

        if (item.Quantity != null)
            _Legacy.Add($"{item.Quantity:0.##}");

        if (item.Volume != null)
            _Legacy.Add($"{item.Volume:0.##} ml");

        if (item.Weight != null)
            _Legacy.Add($"{item.Weight:0.##} g");

        return string.Join(", ", _Legacy);
    }

    public static string DescribeAmount(decimal? amount, long? unit, string? unitAbbreviation)
    {
        if (amount == null)
            return string.Empty;

        var _Abbreviation = string.IsNullOrWhiteSpace(unitAbbreviation)
            ? MeasurementUnits.GetAbbreviation(unit, amount)
            : unitAbbreviation;

        return string.IsNullOrWhiteSpace(_Abbreviation)
            ? $"{amount:0.##}"
            : $"{amount:0.##} {_Abbreviation}";
    }

    /// <summary>
    /// Reads a leading amount and unit off the text and treats the rest as the name. Anything it
    /// cannot make sense of stays in the name untouched, so nobody ever loses what they typed.
    /// </summary>
    public static ParsedShoppingListItem Parse(string? text)
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

        // "500g mince" runs the unit straight onto the number, so the word touching it is tried
        // as a unit before the text is split on spaces.
        var _Attached = _Match.Groups["attached"].Value;

        if (_Attached.Length > 0)
        {
            if (!c_Units.TryGetValue(_Attached, out _Unit))
                return new(null, Capitalise(_Text), null);
        }
        else
        {
            var _Space = _Remainder.IndexOf(' ');
            var _FirstWord = _Space < 0 ? _Remainder : _Remainder[.._Space];

            if (_FirstWord.Length > 0 && c_Units.TryGetValue(_FirstWord, out var _NamedUnit))
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

    /// <summary>
    /// Only the first letter, so "BBQ sauce" and "pak choi" both survive being typed in a hurry.
    /// </summary>
    private static string Capitalise(string text)
        => char.IsLower(text[0]) ? string.Concat(char.ToUpperInvariant(text[0]), text[1..]) : text;

    private static string CollapseWhitespace(string? text)
        => string.IsNullOrWhiteSpace(text) ? string.Empty : WhitespacePattern().Replace(text.Trim(), " ");

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

/// <summary>
/// What someone typed, split into the parts an item is made of.
/// </summary>
/// <param name="Amount">How much to buy, or null when no amount was written.</param>
/// <param name="Name">The thing to buy, always exactly what was typed minus the amount.</param>
/// <param name="Unit">The measurement the amount is in, or null for a plain count.</param>
public record ParsedShoppingListItem(decimal? Amount, string Name, long? Unit);
