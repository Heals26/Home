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
            .Where(a => a.User.UserID == _User.UserID)
            .ToList();

        await output.PresentAssignedActivitiesAsync(_Activities, cancellationToken);
    }

    #endregion Methods

}
