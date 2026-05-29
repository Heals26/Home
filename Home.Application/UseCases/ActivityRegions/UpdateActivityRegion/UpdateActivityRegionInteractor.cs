using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.Activities;
using Home.Application.Services.Persistence;

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
        var _ActivityLogic = serviceFactory.GetService<IActivityLogic>();

        if (!_ActivityLogic.DoesActivityRegionExist(inputPort.ActivityRegionID))
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
