using Home.WebUI.DataAccess.History.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.History;

public partial class HistoryList
{

    #region Fields

    private DateOnly? m_LastHeadingDay;

    #endregion Fields

    #region Properties

    /// <summary>
    /// No day headings and no avatars, for a tile on the board.
    /// </summary>
    [Parameter] public bool Compact { get; set; }

    [Parameter] public string EmptyText { get; set; } = "Nothing yet.";
    [Parameter] public List<HistoryEntryDto> Entries { get; set; } = [];

    /// <summary>
    /// The viewer's "now", for the relative times. Passed in so one render agrees with itself.
    /// </summary>
    [Parameter] public DateTime Now { get; set; }

    [Parameter] public DateOnly Today { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override void OnParametersSet()
        => this.m_LastHeadingDay = null;

    #endregion Lifecycle Methods

    #region Methods

    private static string CategoryDot(HistoryCategory category)
        => category switch
        {
            HistoryCategory.Chores => "bg-week",
            HistoryCategory.Meals => "bg-recipes",
            HistoryCategory.Recipes => "bg-recipes",
            HistoryCategory.Calendar => "bg-calendar",
            HistoryCategory.Shopping => "bg-shopping",
            _ => "bg-household"
        };

    /// <summary>
    /// Rows arrive newest first, so a heading goes in whenever the day changes from the row above.
    /// Rendering is sequential, which is what lets one field carry the last day seen.
    /// </summary>
    private bool StartsNewDay(HistoryEntryDto entry)
    {
        var _Day = DateOnly.FromDateTime(this.ViewerClock.ToLocal(entry.WhenUTC));

        if (this.m_LastHeadingDay == _Day)
            return false;

        this.m_LastHeadingDay = _Day;
        return true;
    }

    #endregion Methods

}
