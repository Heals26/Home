using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Activities.GetActivities;

internal class GetActivitiesInteractor : IInteractor<GetActivitiesInputPort, IGetActivitiesOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetActivitiesInputPort input,
        IGetActivitiesOutputPort output,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Activities = _PersistenceContext.GetEntities<Activity>()
            .Where(a => a.Household.HouseholdID == _Household.HouseholdID)
            // Undated activities sort after everything with a due date, and within a day the
            // "any time today" ones come before the timed ones, matching how a calendar puts
            // all-day entries above the schedule. The ID breaks ties so the board does not
            // reshuffle between loads.
            .OrderBy(a => a.DueDateUTC == null)
            .ThenBy(a => a.DueDateUTC)
            .ThenBy(a => a.DueTime.HasValue)
            .ThenBy(a => a.DueTime)
            .ThenBy(a => a.ActivityID)
            .Select(a => new
            {
                Activity = a,
                a.State,
                a.Status,
                Tags = a.Tags.Select(t => new { ActivityTag = t, t.Tag }),
                a.User
            })
            .ToList()
            .Select(a => a.Activity)
            .ToList();

        await output.PresentActivitiesAsync(_Activities, cancellationToken);
    }

    #endregion Methods

}
