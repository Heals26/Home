using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.CalendarSubscriptions.CreateCalendarSubscription;

public interface ICreateCalendarSubscriptionOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    /// <summary>
    /// The subscription exists. <paramref name="wasFetched"/> says whether the first read of the
    /// feed succeeded; when it did not, the subscription still stands and records why.
    /// </summary>
    Task PresentCalendarSubscriptionCreatedAsync(long calendarSubscriptionID, bool wasFetched, CancellationToken cancellationToken);

    #endregion Methods

}
