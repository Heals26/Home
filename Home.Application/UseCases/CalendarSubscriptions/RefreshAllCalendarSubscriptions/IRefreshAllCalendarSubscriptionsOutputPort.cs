namespace Home.Application.UseCases.CalendarSubscriptions.RefreshAllCalendarSubscriptions;

public interface IRefreshAllCalendarSubscriptionsOutputPort
{

    #region Methods

    Task PresentAllCalendarSubscriptionsRefreshedAsync(IReadOnlyList<long> refreshedHouseholdIDs, int unreachableSubscriptions, CancellationToken cancellationToken);

    #endregion Methods

}
