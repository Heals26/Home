using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.Calendar;

public interface ICalendarSubscriptionLogic
{

    #region Methods

    /// <summary>
    /// Fetches the feed and replaces the subscription's stored occurrences with what it says now.
    /// False when the feed could not be reached, in which case the last good rows are kept and
    /// <see cref="CalendarSubscription.LastError"/> explains. The caller owns saving.
    /// </summary>
    Task<bool> RefreshAsync(CalendarSubscription subscription, CancellationToken cancellationToken);

    #endregion Methods

}
