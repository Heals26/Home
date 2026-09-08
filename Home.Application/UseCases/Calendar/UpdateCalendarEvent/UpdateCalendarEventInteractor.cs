using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.Calendar;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.Calendar.UpdateCalendarEvent;

internal class UpdateCalendarEventInteractor
    : IInteractor<UpdateCalendarEventInputPort, IUpdateCalendarEventOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateCalendarEventInputPort inputPort,
        IUpdateCalendarEventOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<CalendarEvent>>();

        var _Household = _AuthorisationService.GetHousehold();

        // Members are projected because this slice replaces them, and a collection that was never
        // loaded cannot tell EF which rows to drop.
        var _Event = _PersistenceContext.GetEntities<CalendarEvent>()
            .Where(e => e.CalendarEventID == inputPort.CalendarEventID && e.Household.HouseholdID == _Household.HouseholdID)
            .Select(e => new
            {
                Event = e,
                e.Members,
                e.Subscription
            })
            .SingleOrDefault()
            ?.Event;

        if (_Event == null)
        {
            await outputPort.PresentCalendarEventNotFoundAsync(inputPort.CalendarEventID, cancellationToken);
        }
        else if (_Event.Subscription != null)
        {
            await outputPort.PresentCalendarEventReadOnlyAsync(cancellationToken);
        }
        else
        {
            var _Members = _PersistenceContext.GetEntities<User>()
                .Where(u => inputPort.MemberUserIDs.Contains(u.UserID) && u.Household.HouseholdID == _Household.HouseholdID)
                .ToList();

            var _Removed = CalendarEventWriter.Apply(_Event, inputPort, _Members);

            _PersistenceContext.RemoveRange(_Removed);
            _AuditLogic.UpdateAudit(_Event);
            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentCalendarEventNoContentAsync(cancellationToken);
        }
    }

    #endregion Methods

}
