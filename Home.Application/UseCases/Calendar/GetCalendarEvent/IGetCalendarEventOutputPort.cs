using Home.Domain.Entities;

namespace Home.Application.UseCases.Calendar.GetCalendarEvent;

public interface IGetCalendarEventOutputPort
{

    #region Methods

    Task PresentCalendarEventAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken);
    Task PresentCalendarEventNotFoundAsync(long calendarEventID, CancellationToken cancellationToken);

    #endregion Methods

}
