using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityRegions.DeleteActivityRegion;

internal class DeleteActivityRegionInteractor : IInteractor<DeleteActivityRegionInputPort, IDeleteActivityRegionOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteActivityRegionInputPort inputPort,
        IDeleteActivityRegionOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _ActivityRegion = _PersistenceContext.Find<ActivityRegion>(inputPort.ActivityRegionID);

        if (_ActivityRegion == null)
        {
            await outputPort.PresentActivityRegionNotFoundAsync(inputPort.ActivityRegionID, cancellationToken);
            return;
        }

        _PersistenceContext.Remove(_ActivityRegion);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentActivityRegionDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
