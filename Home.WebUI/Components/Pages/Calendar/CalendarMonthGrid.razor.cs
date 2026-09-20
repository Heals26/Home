using Home.WebUI.DataAccess.Calendar.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Calendar;

public partial class CalendarMonthGrid
{

    #region Constants

    private const int MaximumChips = 3;
    private const int MaximumDots = 5;

    #endregion Constants

    #region Properties

    /// <summary>
    /// The 42 days on show, starting on or before the first of the month.
    /// </summary>
    [Parameter] public List<CalendarDayDto> Days { get; set; } = [];

    /// <summary>
    /// The first of the month being shown, so days outside it can be dimmed.
    /// </summary>
    [Parameter] public DateOnly Month { get; set; }

    /// <summary>
    /// The date was tapped: add something on that day.
    /// </summary>
    [Parameter] public EventCallback<DateOnly> OnSelectDay { get; set; }

    [Parameter] public EventCallback<CalendarItemDto> OnSelectItem { get; set; }

    /// <summary>
    /// More is on the day than fits: show it as a list.
    /// </summary>
    [Parameter] public EventCallback<DateOnly> OnShowDay { get; set; }

    [Parameter] public DateOnly Today { get; set; }

    #endregion Properties

    #region Methods

    private static string DotClass(CalendarItemDto item)
        => item.Kind switch
        {
            CalendarItemKind.Meal => "bg-recipes",
            CalendarItemKind.Task => item.IsDone ? "bg-ink-600" : "bg-week",
            CalendarItemKind.Subscribed => "bg-ink-500",
            _ => "bg-calendar"
        };

    #endregion Methods

}
