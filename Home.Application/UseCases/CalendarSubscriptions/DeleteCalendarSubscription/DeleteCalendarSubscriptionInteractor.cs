using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.CalendarSubscriptions.DeleteCalendarSubscription;

internal class DeleteCalendarSubscriptionInteractor
    : IInteractor<DeleteCalendarSubscriptionInputPort, IDeleteCalendarSubscriptionOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteCalendarSubscriptionInputPort inputPort,
        IDeleteCalendarSubscriptionOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<CalendarSubscription>>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Subscription = _PersistenceContext.GetEntities<CalendarSubscription>()
            .Where(s => s.CalendarSubscriptionID == inputPort.CalendarSubscriptionID && s.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Subscription == null)
        {
            await outputPort.PresentCalendarSubscriptionNotFoundAsync(inputPort.CalendarSubscriptionID, cancellationToken);
        }
        else
        {
            // The feed's rows cannot cascade from the subscription (the household already
            // cascades to them), so they go here.
            var _Events = _PersistenceContext.GetEntities<CalendarEvent>()
                .Where(e => e.Subscription != null && e.Subscription.CalendarSubscriptionID == _Subscription.CalendarSubscriptionID)
                .ToList();

            _PersistenceContext.RemoveRange(_Events);
            _AuditLogic.DeleteAudit(_Subscription);
            _PersistenceContext.Remove(_Subscription);
            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentCalendarSubscriptionDeletedAsync(cancellationToken);
        }
    }

    #endregion Methods

}
