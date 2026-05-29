using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
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

        var _ActivityRegion = _PersistenceContext.GetEntities<ActivityRegion>()
            .Where(r => r.ActivityRegionID == inputPort.ActivityRegionID)
            .Select(r => new
            {
                Region = r,
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
