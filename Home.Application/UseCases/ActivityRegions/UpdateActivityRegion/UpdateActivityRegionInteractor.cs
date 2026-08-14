using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityRegions.UpdateActivityRegion;

internal class UpdateActivityRegionInteractor : IInteractor<UpdateActivityRegionInputPort, IUpdateActivityRegionOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateActivityRegionInputPort inputPort,
        IUpdateActivityRegionOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _ActivityLogic = serviceFactory.GetService<IActivityLogic>();

        var _Household = _AuthorisationService.GetHousehold();

        var _ActivityRegionExists = _PersistenceContext.GetEntities<ActivityRegion>()
            .Any(r => r.ActivityRegionID == inputPort.ActivityRegionID
                && r.Activity.Household.HouseholdID == _Household.HouseholdID);

        if (!_ActivityRegionExists)
        {
            await outputPort.PresentActivityRegionNotFoundAsync(inputPort.ActivityRegionID, cancellationToken);
            return;
        }

        _ActivityLogic.UpdateRegion(inputPort);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityRegionNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
