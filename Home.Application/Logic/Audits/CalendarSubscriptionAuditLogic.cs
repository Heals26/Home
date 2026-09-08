using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

/// <summary>
/// Only adding and removing a subscribed calendar are history. Its refreshes are the house talking
/// to itself and would drown the feed.
/// </summary>
public class CalendarSubscriptionAuditLogic(
    IAuthorisationService authorisationService,
    IPersistenceContext persistenceContext,
    TimeProvider timeProvider)
    : AuditBase<CalendarSubscription>(authorisationService, persistenceContext, timeProvider)
{

    #region Properties

    protected override ResourceTypeSE ResourceType => ResourceTypeSE.CalendarSubscription;

    #endregion Properties

    #region Methods

    protected override string Describe(CalendarSubscription subscription, EntityState entityState)
    {
        var _Name = Quote(string.IsNullOrWhiteSpace(subscription.Name) ? "a calendar" : subscription.Name);

        return entityState switch
        {
            EntityState.Added => $"subscribed to the calendar {_Name}",
            EntityState.Deleted => $"unsubscribed from the calendar {_Name}",
            _ => $"changed the calendar {_Name}"
        };
    }

    #endregion Methods

}
