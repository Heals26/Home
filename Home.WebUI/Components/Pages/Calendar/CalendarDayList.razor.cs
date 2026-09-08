using Home.WebUI.DataAccess.Calendar.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Calendar;

public partial class CalendarDayList
{

    #region Properties

    [Parameter] public List<CalendarDayDto> Days { get; set; } = [];
    [Parameter] public EventCallback<DateOnly> OnSelectDay { get; set; }
    [Parameter] public EventCallback<CalendarItemDto> OnSelectItem { get; set; }
    [Parameter] public DateOnly Today { get; set; }

    #endregion Properties

}
