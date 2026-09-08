using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;
using Home.Application.UseCases.Calendar.Models;

namespace Home.Application.UseCases.Calendar.GetCalendar;

public interface IGetCalendarOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentCalendarAsync(IReadOnlyList<CalendarDay> days, CancellationToken cancellationToken);

    #endregion Methods

}
