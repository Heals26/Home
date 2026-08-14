namespace Home.Application.UseCases.Lights.SyncAllLights;

public interface ISyncAllLightsOutputPort
{

    #region Methods

    Task PresentAllLightsSyncedAsync(IReadOnlyList<long> syncedHouseholdIDs, int unavailableHouseholds, CancellationToken cancellationToken);

    #endregion Methods

}
