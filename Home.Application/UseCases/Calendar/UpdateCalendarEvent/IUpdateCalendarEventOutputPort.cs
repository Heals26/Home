using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.Calendar.UpdateCalendarEvent;

public interface IUpdateCalendarEventOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentCalendarEventNoContentAsync(CancellationToken cancellationToken);
    Task PresentCalendarEventNotFoundAsync(long calendarEventID, CancellationToken cancellationToken);

    /// <summary>
    /// The event came from a subscribed calendar and can only be changed there.
    /// </summary>
    Task PresentCalendarEventReadOnlyAsync(CancellationToken cancellationToken);

    #endregion Methods

}
