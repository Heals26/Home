namespace Home.Application.UseCases.LightScenes.ApplyLightScene;

public interface IApplyLightSceneOutputPort
{

    #region Methods

    Task PresentLightSceneAppliedAsync(CancellationToken cancellationToken);
    Task PresentLightSceneNotFoundAsync(long lightSceneID, CancellationToken cancellationToken);
    Task PresentLightsUnavailableAsync(CancellationToken cancellationToken);

    #endregion Methods

}
