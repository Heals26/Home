using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Calendar.GetCalendarEvent;

/// <summary>
/// One event as stored, rule and all, for the editor. The calendar read hands out occurrences,
/// which is the wrong shape to edit a series from.
/// </summary>
internal class GetCalendarEventInteractor
    : IInteractor<GetCalendarEventInputPort, IGetCalendarEventOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetCalendarEventInputPort inputPort,
        IGetCalendarEventOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Event = _PersistenceContext.GetEntities<CalendarEvent>()
            .Where(e => e.CalendarEventID == inputPort.CalendarEventID && e.Household.HouseholdID == _Household.HouseholdID)
            .Select(e => new
            {
                Event = e,
                Members = e.Members.Select(m => new { Member = m, m.User }),
                e.Subscription
            })
            .SingleOrDefault()
            ?.Event;

        if (_Event == null)
            await outputPort.PresentCalendarEventNotFoundAsync(inputPort.CalendarEventID, cancellationToken);
        else
            await outputPort.PresentCalendarEventAsync(_Event, cancellationToken);
    }

    #endregion Methods

}
