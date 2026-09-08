using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

public class CalendarEventAuditLogic(
    IAuthorisationService authorisationService,
    IPersistenceContext persistenceContext,
    TimeProvider timeProvider)
    : AuditBase<CalendarEvent>(authorisationService, persistenceContext, timeProvider)
{

    #region Properties

    protected override ResourceTypeSE ResourceType => ResourceTypeSE.CalendarEvent;

    #endregion Properties

    #region Methods

    protected override string Describe(CalendarEvent calendarEvent, EntityState entityState)
    {
        var _Name = Quote(calendarEvent.Title);

        if (entityState == EntityState.Added)
            return calendarEvent.Frequency == CalendarRecurrenceFrequency.None
                ? $"put {_Name} on the calendar for {calendarEvent.StartDate:ddd d MMM}"
                : $"put {_Name} on the calendar, repeating from {calendarEvent.StartDate:ddd d MMM}";

        if (entityState == EntityState.Deleted)
            return $"took {_Name} off the calendar";

        return $"changed {_Name} on the calendar";
    }

    #endregion Methods

}
