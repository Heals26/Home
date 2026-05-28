using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ActivityContents.GetActivityContents;

internal class GetActivityContentsInteractor : IInteractor<GetActivityContentsInputPort, IGetActivityContentsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetActivityContentsInputPort inputPort,
        IGetActivityContentsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _Region = _PersistenceContext.GetEntities<ActivityRegion>()
            .Where(r => r.ActivityRegionID == inputPort.ActivityRegionID)
            .Select(r => new
            {
                Region = r,
                r.Fields
            })
            .SingleOrDefault()
            ?.Region;

        if (_Region == null)
        {
            await outputPort.PresentActivityRegionNotFoundAsync(inputPort.ActivityRegionID, cancellationToken);
            return;
        }

        await outputPort.PresentActivityContentsAsync(_Region.Fields, cancellationToken);
    }

    #endregion Methods

}
