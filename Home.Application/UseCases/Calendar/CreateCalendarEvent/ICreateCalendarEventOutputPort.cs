using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.Calendar.CreateCalendarEvent;

public interface ICreateCalendarEventOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentCalendarEventCreatedAsync(long calendarEventID, CancellationToken cancellationToken);

    #endregion Methods

}
