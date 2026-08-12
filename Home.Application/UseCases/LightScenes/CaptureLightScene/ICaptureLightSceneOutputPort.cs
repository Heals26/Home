using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.LightScenes.CaptureLightScene;

public interface ICaptureLightSceneOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentLightSceneCapturedAsync(long lightSceneID, int lightCount, CancellationToken cancellationToken);
    Task PresentNoLightsToCaptureAsync(CancellationToken cancellationToken);

    #endregion Methods

}
