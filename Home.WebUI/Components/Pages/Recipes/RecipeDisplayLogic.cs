using Home.WebUI.DataAccess.Recipes.Models;

namespace Home.WebUI.Components.Pages.Recipes;

/// <summary>
/// The one place that decides how a recipe's numbers read, so the book, the detail page,
/// cooking mode and the meal planner never disagree with each other.
/// </summary>
public static class RecipeDisplayLogic
{

    #region Methods

    public static string DescribeAmount(RecipeIngredientDto ingredient)
    {
        if (ingredient.Amount == null)
            return string.Empty;

        var _Abbreviation = string.IsNullOrWhiteSpace(ingredient.UnitAbbreviation)
            ? MeasurementUnits.GetAbbreviation(ingredient.Unit, ingredient.Amount)
            : ingredient.UnitAbbreviation;

        return string.IsNullOrWhiteSpace(_Abbreviation)
            ? $"{ingredient.Amount:0.##}"
            : $"{ingredient.Amount:0.##} {_Abbreviation}";
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
