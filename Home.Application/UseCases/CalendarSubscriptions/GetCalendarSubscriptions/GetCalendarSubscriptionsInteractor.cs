using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.CalendarSubscriptions.GetCalendarSubscriptions;

internal class GetCalendarSubscriptionsInteractor
    : IInteractor<GetCalendarSubscriptionsInputPort, IGetCalendarSubscriptionsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetCalendarSubscriptionsInputPort inputPort,
        IGetCalendarSubscriptionsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Subscriptions = _PersistenceContext.GetEntities<CalendarSubscription>()
            .Where(s => s.Household.HouseholdID == _Household.HouseholdID)
            .OrderBy(s => s.Name)
            .ThenBy(s => s.CalendarSubscriptionID)
            .ToList();

        await outputPort.PresentCalendarSubscriptionsAsync(_Subscriptions, cancellationToken);
    }

    #endregion Methods

}
