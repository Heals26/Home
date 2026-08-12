namespace Home.Application.UseCases.Lights.SyncLights;

public interface ISyncLightsOutputPort
{

    #region Methods

    Task PresentLightsSyncedAsync(int added, int updated, int removed, CancellationToken cancellationToken);
    Task PresentLightsUnavailableAsync(CancellationToken cancellationToken);

    #endregion Methods

}
