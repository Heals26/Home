using Home.WebUI.DataAccess.Recipes.Models;

namespace Home.WebUI.Components.Pages.Recipes;

/// <summary>
/// The one place that decides how a recipe's numbers read, so the book, the detail page,
/// cooking mode and the meal planner never disagree with each other.
/// </summary>
public static class RecipeDisplayLogic
{

    #region Methods

    /// <summary>
    /// Amounts written before units existed only had a bare quantity, a volume in millilitres
    /// or a weight in grams, so those are still read when there is no amount to show.
    /// </summary>
    public static string DescribeAmount(RecipeIngredientDto ingredient)
    {
        if (ingredient.Amount != null)
        {
            var _Abbreviation = string.IsNullOrWhiteSpace(ingredient.UnitAbbreviation)
                ? MeasurementUnits.GetAbbreviation(ingredient.Unit)
                : ingredient.UnitAbbreviation;

            return string.IsNullOrWhiteSpace(_Abbreviation)
                ? $"{ingredient.Amount:0.##}"
                : $"{ingredient.Amount:0.##} {_Abbreviation}";
        }

        List<string> _Legacy = [];

        if (ingredient.Quantity != null)
            _Legacy.Add($"{ingredient.Quantity:0.##}");

        if (ingredient.Volume != null)
            _Legacy.Add($"{ingredient.Volume:0.##} ml");

        if (ingredient.Weight != null)
            _Legacy.Add($"{ingredient.Weight:0.##} g");

        return string.Join(", ", _Legacy);
    }

    public static string DescribeComplexity(long? complexity)
        => RecipeComplexities.GetName(complexity);

    public static string DescribeMinutes(int minutes)
        => minutes switch
        {
            < 60 => $"{minutes} min",
            60 => "1 hr",
            _ when minutes % 60 == 0 => $"{minutes / 60} hr",
            _ => $"{minutes / 60} hr {minutes % 60} min"
        };

    /// <summary>
    /// Only absolute http or https addresses are ever put in an img tag — anything else is a
    /// scheme the tablet has no business following.
    /// </summary>
    public static bool IsAWebImage(string? imageUrl)
        => !string.IsNullOrWhiteSpace(imageUrl)
            && Uri.TryCreate(imageUrl, UriKind.Absolute, out var _Uri)
            && (_Uri.Scheme == Uri.UriSchemeHttp || _Uri.Scheme == Uri.UriSchemeHttps);

    #endregion Methods

}
