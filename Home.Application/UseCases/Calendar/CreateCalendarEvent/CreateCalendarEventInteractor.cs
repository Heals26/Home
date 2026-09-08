using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.Calendar;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.Calendar.CreateCalendarEvent;

internal class CreateCalendarEventInteractor
    : IInteractor<CreateCalendarEventInputPort, ICreateCalendarEventOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateCalendarEventInputPort inputPort,
        ICreateCalendarEventOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<CalendarEvent>>();

        var _Household = _AuthorisationService.GetHousehold();

        // Scoped to the household, so a member ID from another family simply is not found and is
        // left off rather than putting a stranger on the event.
        var _Members = _PersistenceContext.GetEntities<User>()
            .Where(u => inputPort.MemberUserIDs.Contains(u.UserID) && u.Household.HouseholdID == _Household.HouseholdID)
            .ToList();

        var _Event = new CalendarEvent()
        {
            Household = _Household
        };

        _ = CalendarEventWriter.Apply(_Event, inputPort, _Members);

        _PersistenceContext.Add(_Event);
        _AuditLogic.AddAudit(_Event);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentCalendarEventCreatedAsync(_Event.CalendarEventID, cancellationToken);
    }

    #endregion Methods

}
