using Home.WebUI.DataAccess.Calendar.Models;

namespace Home.WebUI.DataAccess.Calendar.GetCalendar;

public class GetCalendarWebAppResponse
{

    #region Properties

    /// <summary>
    /// One entry per day asked for, in order, including days with nothing on them.
    /// </summary>
    public List<CalendarDayDto> Days { get; set; } = [];

    #endregion Properties

}
