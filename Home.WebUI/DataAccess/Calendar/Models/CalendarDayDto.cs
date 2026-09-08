namespace Home.WebUI.DataAccess.Calendar.Models;

/// <summary>
/// One day and everything on it, all-day items first, then by time.
/// </summary>
public class CalendarDayDto
{

    #region Properties

    public DateOnly Date { get; set; }
    public List<CalendarItemDto> Items { get; set; } = [];

    #endregion Properties

}
