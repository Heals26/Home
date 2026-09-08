using Home.WebUI.DataAccess.History.Models;
using System.Globalization;

namespace Home.WebUI.Infrastructure.History;

/// <summary>
/// How the feed writes its rows, in one place so the page, the board tile and a chore's own history
/// card never disagree about what "2h ago" or "Chores" looks like.
/// </summary>
public static class HistoryFormat
{

    #region Fields

    /// <summary>
    /// What the feed shows until a device chooses otherwise. Shopping is off because a list is
    /// touched more often than anything else in the house and would drown the rest.
    /// </summary>
    public static readonly IReadOnlyList<HistoryCategory> DefaultCategories =
    [
        HistoryCategory.Chores, HistoryCategory.Meals, HistoryCategory.Recipes, HistoryCategory.Calendar, HistoryCategory.Members
    ];

    public static readonly string PreferenceKey = "history-categories";

    #endregion Fields

    #region Methods

    public static string CategoryLabel(HistoryCategory category)
        => category switch
        {
            HistoryCategory.Chores => "Chores",
            HistoryCategory.Meals => "Meals",
            HistoryCategory.Recipes => "Recipes",
            HistoryCategory.Calendar => "Calendar",
            HistoryCategory.Shopping => "Shopping",
            _ => "Members"
        };

    public static string DayHeading(DateOnly date, DateOnly today)
    {
        if (date == today)
            return "Today";

        if (date == today.AddDays(-1))
            return "Yesterday";

        return date.Year == today.Year
            ? date.ToString("dddd d MMMM", CultureInfo.CurrentCulture)
            : date.ToString("d MMMM yyyy", CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// The screen the row leads to: the thing itself where it has a page, its area otherwise.
    /// </summary>
    public static string Href(HistoryEntryDto entry)
        => entry.Category switch
        {
            HistoryCategory.Chores => $"/activities/{entry.EntityID}",
            HistoryCategory.Recipes => $"/recipes/{entry.EntityID}",
            HistoryCategory.Meals => "/meal-plan",
            HistoryCategory.Calendar => "/calendar",
            HistoryCategory.Shopping => "/shopping-lists",
            _ => "/settings"
        };

    public static string Initials(string who)
    {
        var _Parts = who.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return _Parts.Length switch
        {
            0 => "?",
            1 => _Parts[0][..1].ToUpperInvariant(),
            _ => string.Concat(_Parts[0][..1], _Parts[^1][..1]).ToUpperInvariant()
        };
    }

    public static IReadOnlyList<HistoryCategory> ParseCategories(string stored)
        => string.IsNullOrWhiteSpace(stored)
            ? DefaultCategories
            : [.. stored.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Enum.TryParse<HistoryCategory>(s, out var _Category) ? _Category : (HistoryCategory?)null)
                .Where(c => c != null)
                .Select(c => c!.Value)];

    public static string SerialiseCategories(IEnumerable<HistoryCategory> categories)
        => string.Join(",", categories.Select(c => c.ToString()));

    /// <summary>
    /// "just now", "12m ago", "3h ago", then a clock time for the same day and a date beyond it.
    /// </summary>
    public static string When(DateTime localWhen, DateTime localNow)
    {
        var _Ago = localNow - localWhen;

        if (_Ago < TimeSpan.FromMinutes(1))
            return "just now";

        if (_Ago < TimeSpan.FromHours(1))
            return $"{(int)_Ago.TotalMinutes}m ago";

        if (_Ago < TimeSpan.FromHours(6))
            return $"{(int)_Ago.TotalHours}h ago";

        if (localWhen.Date == localNow.Date)
            return localWhen.ToString("h:mm tt", CultureInfo.InvariantCulture).ToLowerInvariant();

        return localWhen.ToString("d MMM, h:mm tt", CultureInfo.InvariantCulture).ToLowerInvariant();
    }

    #endregion Methods

}
