using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.Calendar.DeleteCalendarEvent;

internal class DeleteCalendarEventInteractor
    : IInteractor<DeleteCalendarEventInputPort, IDeleteCalendarEventOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteCalendarEventInputPort inputPort,
        IDeleteCalendarEventOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<CalendarEvent>>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Event = _PersistenceContext.GetEntities<CalendarEvent>()
            .Where(e => e.CalendarEventID == inputPort.CalendarEventID && e.Household.HouseholdID == _Household.HouseholdID)
            .Select(e => new
            {
                Event = e,
                e.Exceptions,
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
            if (inputPort.OccurrenceDate is { } _OccurrenceDate && _Event.Frequency != CalendarRecurrenceFrequency.None)
            {
                // Skipping the same day twice is the same skip.
                if (!_Event.Exceptions.Any(x => x.OccurrenceDate == _OccurrenceDate))
                {
                    _PersistenceContext.Add(new CalendarEventException() { CalendarEvent = _Event, OccurrenceDate = _OccurrenceDate });
                    _AuditLogic.UpdateAudit(_Event, $"skipped '{_Event.Title}' on {_OccurrenceDate:ddd d MMM}");
                }
            }
            else
            {
                _AuditLogic.DeleteAudit(_Event);
                _PersistenceContext.Remove(_Event);
            }

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentCalendarEventDeletedAsync(cancellationToken);
        }
    }

    #endregion Methods

}
