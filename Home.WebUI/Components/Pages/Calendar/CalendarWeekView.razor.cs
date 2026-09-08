using Home.WebUI.DataAccess.Calendar.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Calendar;

public partial class CalendarWeekView
{

    #region Properties

    /// <summary>
    /// The seven days on show, Monday first.
    /// </summary>
    [Parameter] public List<CalendarDayDto> Days { get; set; } = [];

    [Parameter] public EventCallback<DateOnly> OnSelectDay { get; set; }
    [Parameter] public EventCallback<CalendarItemDto> OnSelectItem { get; set; }
    [Parameter] public DateOnly Today { get; set; }

    #endregion Properties

}
