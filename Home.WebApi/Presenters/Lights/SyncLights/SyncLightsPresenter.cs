using AutoMapper;
using Home.Application.UseCases.Lights.SyncLights;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Lights.SyncLights;

namespace Home.WebApi.Presenters.Lights.SyncLights;

public class SyncLightsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISyncLightsOutputPort
{

    #region Methods

    Task ISyncLightsOutputPort.PresentLightsSyncedAsync(int added, int updated, int removed, CancellationToken cancellationToken)
        => this.OkAsync(new SyncLightsApiResponse()
        {
            Added = added,
            Updated = updated,
            Removed = removed
        }, cancellationToken);

    Task ISyncLightsOutputPort.PresentLightsUnavailableAsync(CancellationToken cancellationToken)
        => this.ServiceUnavailableAsync("The lighting service could not be reached", cancellationToken);

    #endregion Methods

}
