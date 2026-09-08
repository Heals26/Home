using Home.WebUI.DataAccess.Calendar.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Dashboard;

public partial class DashboardAgenda
{

    #region Constants

    private const int NextDayCount = 3;

    #endregion Constants

    #region Properties

    /// <summary>
    /// Today first, then the days after it. Null while loading.
    /// </summary>
    [Parameter] public List<CalendarDayDto>? Days { get; set; }

    [Parameter] public EventCallback<CalendarItemDto> OnCompleteTask { get; set; }
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

    private IEnumerable<CalendarDayDto> NextDays()
        => (this.Days ?? []).Where(d => d.Date > this.Today).Take(NextDayCount);

    private List<CalendarItemDto> TodayItems()
        => (this.Days ?? []).FirstOrDefault(d => d.Date == this.Today)?.Items ?? [];

    #endregion Methods

}
