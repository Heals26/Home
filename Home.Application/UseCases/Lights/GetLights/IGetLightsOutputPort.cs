using Home.Application.Services.Lights;

namespace Home.Application.UseCases.Lights.GetLights;

public interface IGetLightsOutputPort
{

    #region Methods

    Task PresentLightsAsync(IReadOnlyList<LightSnapshot> lights, CancellationToken cancellationToken);
    Task PresentLightsUnavailableAsync(CancellationToken cancellationToken);

    #endregion Methods

}
