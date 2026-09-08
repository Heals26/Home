using System.Globalization;

namespace Home.WebUI.Infrastructure.Recipes;

/// <summary>
/// What the planner says about how long it has been. Three thresholds and their words live here so
/// the picker, the recipe page and the nudge agree.
/// </summary>
public static class MealMemory
{

    #region Fields

    /// <summary>
    /// A recipe had within this many days gets a quiet "you just had this" when picked again.
    /// </summary>
    public static readonly int RecentDays = 3;

    /// <summary>
    /// A recipe not had for this long is due a turn.
    /// </summary>
    public static readonly int DueATurnDays = 180;

    #endregion Fields

    #region Methods

    public static string Describe(DateOnly? lastHad, DateOnly today)
    {
        if (lastHad == null)
            return "Never had";

        var _Days = today.DayNumber - lastHad.Value.DayNumber;

        return _Days switch
        {
            0 => "Had today",
            1 => "Had yesterday",
            < 14 => $"Had {_Days} days ago",
            < 60 => $"Had {_Days / 7} weeks ago",
            _ => $"Last had {lastHad.Value.ToString(lastHad.Value.Year == today.Year ? "d MMM" : "MMM yyyy", CultureInfo.CurrentCulture)}"
        };
    }

    public static bool IsDueATurn(DateOnly? lastHad, DateOnly today)
        => lastHad != null && today.DayNumber - lastHad.Value.DayNumber >= DueATurnDays;

    public static bool IsRecent(DateOnly? lastHad, DateOnly today)
        => lastHad != null && today.DayNumber - lastHad.Value.DayNumber <= RecentDays;

    #endregion Methods

}
