using AutoMapper;
using Home.Application.UseCases.Calendar.UpdateCalendarEvent;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Calendar.UpdateCalendarEvent;

public class UpdateCalendarEventPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateCalendarEventOutputPort
{

    #region Methods

    Task IUpdateCalendarEventOutputPort.PresentCalendarEventNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUpdateCalendarEventOutputPort.PresentCalendarEventNotFoundAsync(long calendarEventID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Calendar Event {calendarEventID} Not Found", cancellationToken);

    Task IUpdateCalendarEventOutputPort.PresentCalendarEventReadOnlyAsync(CancellationToken cancellationToken)
        => this.UnprocessableContent(CalendarEventReadOnlyProblem.Create(), cancellationToken);

    #endregion Methods

}
