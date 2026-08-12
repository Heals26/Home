using AutoMapper;
using Home.Application.UseCases.LightScenes.ApplyLightScene;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.LightScenes.ApplyLightScene;

public class ApplyLightScenePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IApplyLightSceneOutputPort
{

    #region Methods

    Task IApplyLightSceneOutputPort.PresentLightSceneAppliedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IApplyLightSceneOutputPort.PresentLightSceneNotFoundAsync(long lightSceneID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Light Scene {lightSceneID} Not Found", cancellationToken);

    Task IApplyLightSceneOutputPort.PresentLightsUnavailableAsync(CancellationToken cancellationToken)
        => this.ServiceUnavailableAsync("The lighting service could not be reached", cancellationToken);

    #endregion Methods

}
