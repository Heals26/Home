namespace Home.Application.UseCases.Lights.StartLightEffect;

public interface IStartLightEffectOutputPort
{

    #region Methods

    Task PresentEffectStartedAsync(CancellationToken cancellationToken);
    Task PresentLightGroupNotFoundAsync(long lightGroupID, CancellationToken cancellationToken);
    Task PresentLightsUnavailableAsync(CancellationToken cancellationToken);

    #endregion Methods

}
