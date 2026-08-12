using AutoMapper;
using Home.Application.UseCases.Lights.StartLightEffect;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Lights.StartLightEffect;

public class StartLightEffectPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IStartLightEffectOutputPort
{

    #region Methods

    Task IStartLightEffectOutputPort.PresentEffectStartedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IStartLightEffectOutputPort.PresentLightGroupNotFoundAsync(long lightGroupID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Light Group {lightGroupID} Not Found", cancellationToken);

    Task IStartLightEffectOutputPort.PresentLightsUnavailableAsync(CancellationToken cancellationToken)
        => this.ServiceUnavailableAsync("The lighting service could not be reached", cancellationToken);

    #endregion Methods

}
