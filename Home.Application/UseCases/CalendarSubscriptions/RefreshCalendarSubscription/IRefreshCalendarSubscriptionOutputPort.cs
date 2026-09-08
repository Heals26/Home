namespace Home.Application.UseCases.CalendarSubscriptions.RefreshCalendarSubscription;

public interface IRefreshCalendarSubscriptionOutputPort
{

    #region Methods

    Task PresentCalendarSubscriptionNotFoundAsync(long calendarSubscriptionID, CancellationToken cancellationToken);
    Task PresentCalendarSubscriptionRefreshedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// The feed could not be read. The last good occurrences stand and the reason is recorded.
    /// </summary>
    Task PresentCalendarSubscriptionUnreachableAsync(CancellationToken cancellationToken);

    #endregion Methods

}
