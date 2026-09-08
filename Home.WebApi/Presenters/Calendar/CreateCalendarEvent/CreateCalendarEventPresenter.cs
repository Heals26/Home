using AutoMapper;
using Home.Application.UseCases.Calendar.CreateCalendarEvent;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Calendar.CreateCalendarEvent;

namespace Home.WebApi.Presenters.Calendar.CreateCalendarEvent;

public class CreateCalendarEventPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateCalendarEventOutputPort
{

    #region Methods

    Task ICreateCalendarEventOutputPort.PresentCalendarEventCreatedAsync(long calendarEventID, CancellationToken cancellationToken)
        => this.CreatedAsync(calendarEventID, new CreateCalendarEventApiResponse() { CalendarEventID = calendarEventID }, cancellationToken);

    #endregion Methods

}
