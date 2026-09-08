using Home.WebApi.UseCases.Calendar.Models;

namespace Home.WebApi.UseCases.Calendar.GetCalendar;

public class GetCalendarApiResponse
{

    #region Properties

    /// <summary>
    /// One entry per day asked for, in order, including days with nothing on them.
    /// </summary>
    public List<CalendarDayDto> Days { get; set; } = [];

    #endregion Properties

}
