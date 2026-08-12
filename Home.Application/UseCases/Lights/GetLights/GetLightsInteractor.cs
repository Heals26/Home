using CleanArchitecture.Mediator;
using Home.Application.Services.Lights;

namespace Home.Application.UseCases.Lights.GetLights;

internal class GetLightsInteractor : IInteractor<GetLightsInputPort, IGetLightsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetLightsInputPort inputPort,
        IGetLightsOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _LightService = serviceFactory.GetService<ILightService>();

        var _Lights = await _LightService.GetLightsAsync(cancellationToken);

        if (_Lights == null)
            await outputPort.PresentLightsUnavailableAsync(cancellationToken);
        else
            await outputPort.PresentLightsAsync(_Lights, cancellationToken);
    }

    #endregion Methods

}
