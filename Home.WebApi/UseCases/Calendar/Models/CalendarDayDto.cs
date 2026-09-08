namespace Home.WebApi.UseCases.Calendar.Models;

public class CalendarDayDto
{

    #region Properties

    public DateOnly Date { get; set; }
    public List<CalendarItemDto> Items { get; set; } = [];

    #endregion Properties

}
