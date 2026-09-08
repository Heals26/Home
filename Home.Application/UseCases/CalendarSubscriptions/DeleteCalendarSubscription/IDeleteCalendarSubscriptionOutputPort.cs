namespace Home.Application.UseCases.CalendarSubscriptions.DeleteCalendarSubscription;

public interface IDeleteCalendarSubscriptionOutputPort
{

    #region Methods

    Task PresentCalendarSubscriptionDeletedAsync(CancellationToken cancellationToken);
    Task PresentCalendarSubscriptionNotFoundAsync(long calendarSubscriptionID, CancellationToken cancellationToken);

    #endregion Methods

}
