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
            .ToList();

        await output.PresentActivitiesAsync(_Activities, cancellationToken);
    }

    #endregion Methods

}
