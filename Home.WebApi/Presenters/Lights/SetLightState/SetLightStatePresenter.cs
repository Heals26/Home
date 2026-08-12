using AutoMapper;
using Home.Application.UseCases.Lights.SetLightState;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.Lights.SetLightState;

public class SetLightStatePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISetLightStateOutputPort
{

    #region Methods

    Task ISetLightStateOutputPort.PresentLightNotFoundAsync(string lightID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Light {lightID} Not Found", cancellationToken);

    Task ISetLightStateOutputPort.PresentLightStateSetAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task ISetLightStateOutputPort.PresentLightsUnavailableAsync(CancellationToken cancellationToken)
        => this.ServiceUnavailableAsync("The lighting service could not be reached", cancellationToken);

    // An empty body is a no-op rather than an error — the caller asked for nothing and got it.
    Task ISetLightStateOutputPort.PresentNothingToChangeAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
