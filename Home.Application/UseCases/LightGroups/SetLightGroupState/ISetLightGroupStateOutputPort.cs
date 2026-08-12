namespace Home.Application.UseCases.LightGroups.SetLightGroupState;

public interface ISetLightGroupStateOutputPort
{

    #region Methods

    Task PresentLightGroupNotFoundAsync(long lightGroupID, CancellationToken cancellationToken);
    Task PresentLightGroupStateSetAsync(CancellationToken cancellationToken);
    Task PresentLightsUnavailableAsync(CancellationToken cancellationToken);
    Task PresentNothingToChangeAsync(CancellationToken cancellationToken);

    #endregion Methods

}
