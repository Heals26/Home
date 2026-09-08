using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Calendar;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.CalendarSubscriptions.RefreshCalendarSubscription;

internal class RefreshCalendarSubscriptionInteractor
    : IInteractor<RefreshCalendarSubscriptionInputPort, IRefreshCalendarSubscriptionOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        RefreshCalendarSubscriptionInputPort inputPort,
        IRefreshCalendarSubscriptionOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _SubscriptionLogic = serviceFactory.GetService<ICalendarSubscriptionLogic>();

        var _Household = _AuthorisationService.GetHousehold();

        // The household is projected because the refresh writes new rows that hang off it.
        var _Subscription = _PersistenceContext.GetEntities<CalendarSubscription>()
            .Where(s => s.CalendarSubscriptionID == inputPort.CalendarSubscriptionID && s.Household.HouseholdID == _Household.HouseholdID)
            .Select(s => new
            {
                Subscription = s,
                s.Household
            })
            .SingleOrDefault()
            ?.Subscription;

        if (_Subscription == null)
        {
            await outputPort.PresentCalendarSubscriptionNotFoundAsync(inputPort.CalendarSubscriptionID, cancellationToken);
        }
        else
        {
            var _WasFetched = await _SubscriptionLogic.RefreshAsync(_Subscription, cancellationToken);

            // Saved either way: a failure is recorded on the row for the Settings card to show.
            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            if (_WasFetched)
                await outputPort.PresentCalendarSubscriptionRefreshedAsync(cancellationToken);
            else
                await outputPort.PresentCalendarSubscriptionUnreachableAsync(cancellationToken);
        }
    }

    #endregion Methods

}
