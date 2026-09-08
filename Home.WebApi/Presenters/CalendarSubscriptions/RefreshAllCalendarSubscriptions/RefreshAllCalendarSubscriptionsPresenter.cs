using AutoMapper;
using Home.Application.UseCases.CalendarSubscriptions.RefreshAllCalendarSubscriptions;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.CalendarSubscriptions.RefreshAllCalendarSubscriptions;

/// <summary>
/// Not reached over HTTP. The background runner reads the outcome off it directly, so this
/// presents into properties rather than an <c>IActionResult</c>.
/// </summary>
public class RefreshAllCalendarSubscriptionsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IRefreshAllCalendarSubscriptionsOutputPort
{

    #region Properties

    public IReadOnlyList<long> RefreshedHouseholdIDs { get; private set; } = [];

    public int UnreachableSubscriptions { get; private set; }

    #endregion Properties

    #region Methods

    Task IRefreshAllCalendarSubscriptionsOutputPort.PresentAllCalendarSubscriptionsRefreshedAsync(IReadOnlyList<long> refreshedHouseholdIDs, int unreachableSubscriptions, CancellationToken cancellationToken)
    {
        this.RefreshedHouseholdIDs = refreshedHouseholdIDs;
        this.UnreachableSubscriptions = unreachableSubscriptions;

        return Task.CompletedTask;
    }

    #endregion Methods

}
