namespace Home.Application.UseCases.LightScenes.DeleteLightScene;

public interface IDeleteLightSceneOutputPort
{

    #region Methods

    Task PresentLightSceneDeletedAsync(CancellationToken cancellationToken);
    Task PresentLightSceneNotFoundAsync(long lightSceneID, CancellationToken cancellationToken);

    #endregion Methods

}
