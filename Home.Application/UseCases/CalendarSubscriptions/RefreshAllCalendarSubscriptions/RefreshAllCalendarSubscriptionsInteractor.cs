using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Calendar;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.CalendarSubscriptions.RefreshAllCalendarSubscriptions;

/// <summary>
/// The background counterpart of RefreshCalendarSubscription: every household's feeds, on a
/// timer. Deliberately does not use <c>IAuthorisationService</c>; there is no signed-in user
/// behind a tick.
/// </summary>
internal class RefreshAllCalendarSubscriptionsInteractor
    : IInteractor<RefreshAllCalendarSubscriptionsInputPort, IRefreshAllCalendarSubscriptionsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        RefreshAllCalendarSubscriptionsInputPort inputPort,
        IRefreshAllCalendarSubscriptionsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _SubscriptionLogic = serviceFactory.GetService<ICalendarSubscriptionLogic>();

        var _Subscriptions = _PersistenceContext.GetEntities<CalendarSubscription>()
            .Select(s => new
            {
                Subscription = s,
                s.Household
            })
            .ToList()
            .Select(s => s.Subscription)
            .ToList();

        var _RefreshedHouseholdIDs = new HashSet<long>();
        var _Unreachable = 0;

        foreach (var _Subscription in _Subscriptions)
        {
            var _WasFetched = await _SubscriptionLogic.RefreshAsync(_Subscription, cancellationToken);

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            if (_WasFetched)
                _ = _RefreshedHouseholdIDs.Add(_Subscription.Household.HouseholdID);
            else
                _Unreachable++;
        }

        await outputPort.PresentAllCalendarSubscriptionsRefreshedAsync([.. _RefreshedHouseholdIDs], _Unreachable, cancellationToken);
    }

    #endregion Methods

}
