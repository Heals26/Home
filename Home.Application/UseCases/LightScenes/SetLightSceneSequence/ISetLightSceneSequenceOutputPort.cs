namespace Home.Application.UseCases.LightScenes.SetLightSceneSequence;

public interface ISetLightSceneSequenceOutputPort
{

    #region Methods

    Task PresentLightSceneNotFoundAsync(long lightSceneID, CancellationToken cancellationToken);
    Task PresentLightSceneSequenceSetAsync(CancellationToken cancellationToken);

    #endregion Methods

}
