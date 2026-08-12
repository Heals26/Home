namespace Home.Application.UseCases.Lights.SetLightState;

public interface ISetLightStateOutputPort
{

    #region Methods

    Task PresentLightNotFoundAsync(string lightID, CancellationToken cancellationToken);
    Task PresentLightStateSetAsync(CancellationToken cancellationToken);
    Task PresentLightsUnavailableAsync(CancellationToken cancellationToken);
    Task PresentNothingToChangeAsync(CancellationToken cancellationToken);

    #endregion Methods

}
