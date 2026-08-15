using Home.WebUI.DataAccess.Activities.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Activities;

public partial class ActivityCalendarView
{

    #region Properties

    [Parameter] public List<ActivitySummaryDto> Activities { get; set; } = [];
    /// <summary>
    /// Seven days for the week view, one for the day view. The page owns which days these are so
    /// the clock is only read in one place.
    /// </summary>
    [Parameter] public List<DateTime> Days { get; set; } = [];
    [Parameter] public EventCallback<ActivitySummaryDto> OnEdit { get; set; }
    [Parameter] public EventCallback<ActivitySummaryDto> OnOpen { get; set; }
    [Parameter] public EventCallback<ActivityCompletion> OnToggleComplete { get; set; }
    [Parameter] public DateTime Today { get; set; }

    #endregion Properties

    #region Methods

    private string GetLaneClasses(bool isToday)
    {
        // Seven lanes have to fit a tablet screen at once or the week stops being glanceable, so
        // they share the width rather than each claiming a fixed one.
        var _Base = "flex flex-col bg-ink-900/60 border rounded-xl max-h-full";
        var _Width = this.Days.Count == 1 ? "flex-1 min-w-0 max-w-2xl" : "flex-1 min-w-[9rem]";
        var _Border = isToday ? "border-week/40" : "border-ink-800";

        return $"{_Base} {_Width} {_Border}";
    }

    #endregion Methods

}
