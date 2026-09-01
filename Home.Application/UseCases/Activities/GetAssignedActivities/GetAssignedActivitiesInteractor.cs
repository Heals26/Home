using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Activities.GetAssignedActivities;

internal class GetAssignedActivitiesInteractor : IInteractor<GetAssignedActivitiesInputPort, IGetAssignedActivitiesOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetAssignedActivitiesInputPort input,
        IGetAssignedActivitiesOutputPort output,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _User = _AuthorisationService.GetUser();

        var _Activities = _PersistenceContext.GetEntities<Activity>()
            .Where(a => a.User != null && a.User.UserID == _User.UserID)
            .Select(a => new
            {
                Activity = a,
                a.State,
                Tags = a.Tags.Select(t => new { ActivityTag = t, t.Tag }),
                a.User
            })
            .ToList()
            .Select(a => a.Activity)
            .ToList();

        await output.PresentAssignedActivitiesAsync(_Activities, cancellationToken);
    }

    #endregion Methods

}
