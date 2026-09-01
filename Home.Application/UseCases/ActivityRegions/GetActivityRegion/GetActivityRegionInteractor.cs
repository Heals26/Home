using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityRegions.GetActivityRegion;

internal class GetActivityRegionInteractor : IInteractor<GetActivityRegionInputPort, IGetActivityRegionOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetActivityRegionInputPort inputPort,
        IGetActivityRegionOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ActivityRegion = _PersistenceContext.GetEntities<ActivityRegion>()
            .Where(r => r.ActivityRegionID == inputPort.ActivityRegionID
                && r.Activity.Household.HouseholdID == _Household.HouseholdID)
            .Select(r => new
            {
                Region = r,
                r.CardSection,
                r.Fields
            })
            .SingleOrDefault()
            ?.Region;

        if (_ActivityRegion == null)
            await outputPort.PresentActivityRegionNotFoundAsync(inputPort.ActivityRegionID, cancellationToken);
        else
            await outputPort.PresentActivityRegionAsync(_ActivityRegion, cancellationToken);
    }

    #endregion Methods

}
