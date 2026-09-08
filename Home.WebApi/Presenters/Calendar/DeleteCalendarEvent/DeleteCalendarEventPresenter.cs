using AutoMapper;
using Home.Application.UseCases.Calendar.DeleteCalendarEvent;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Calendar.DeleteCalendarEvent;

public class DeleteCalendarEventPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteCalendarEventOutputPort
{

    #region Methods

    Task IDeleteCalendarEventOutputPort.PresentCalendarEventDeletedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IDeleteCalendarEventOutputPort.PresentCalendarEventNotFoundAsync(long calendarEventID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Calendar Event {calendarEventID} Not Found", cancellationToken);

    Task IDeleteCalendarEventOutputPort.PresentCalendarEventReadOnlyAsync(CancellationToken cancellationToken)
        => this.UnprocessableContent(CalendarEventReadOnlyProblem.Create(), cancellationToken);

    #endregion Methods

}
