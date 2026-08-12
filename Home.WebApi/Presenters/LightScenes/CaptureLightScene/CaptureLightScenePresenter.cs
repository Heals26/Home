using AutoMapper;
using Home.Application.UseCases.LightScenes.CaptureLightScene;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.LightScenes.CaptureLightScene;

namespace Home.WebApi.Presenters.LightScenes.CaptureLightScene;

public class CaptureLightScenePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICaptureLightSceneOutputPort
{

    #region Methods

    Task ICaptureLightSceneOutputPort.PresentLightSceneCapturedAsync(long lightSceneID, int lightCount, CancellationToken cancellationToken)
        => this.CreatedAsync(lightSceneID, new CaptureLightSceneApiResponse()
        {
            LightSceneID = lightSceneID,
            LightCount = lightCount
        }, cancellationToken);

    Task ICaptureLightSceneOutputPort.PresentNoLightsToCaptureAsync(CancellationToken cancellationToken)
        => this.NotFoundAsync("There are no lights to capture — sync first", cancellationToken);

    #endregion Methods

}
