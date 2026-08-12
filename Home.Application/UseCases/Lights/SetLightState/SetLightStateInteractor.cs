using CleanArchitecture.Mediator;
using Home.Application.Services.Lights;

namespace Home.Application.UseCases.Lights.SetLightState;

internal class SetLightStateInteractor : IInteractor<SetLightStateInputPort, ISetLightStateOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SetLightStateInputPort inputPort,
        ISetLightStateOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _LightService = serviceFactory.GetService<ILightService>();

        var _Change = new LightStateChange(
            inputPort.IsOn,
            inputPort.Brightness,
            inputPort.Hue,
            inputPort.Saturation,
            inputPort.Kelvin);

        if (_Change.IsEmpty)
        {
            await outputPort.PresentNothingToChangeAsync(cancellationToken);
            return;
        }

        var _Result = await _LightService.SetStateAsync(inputPort.LightID, _Change, cancellationToken);

        if (_Result == LightCommandResult.LightNotFound)
            await outputPort.PresentLightNotFoundAsync(inputPort.LightID, cancellationToken);
        else if (_Result == LightCommandResult.Unavailable)
            await outputPort.PresentLightsUnavailableAsync(cancellationToken);
        else
            await outputPort.PresentLightStateSetAsync(cancellationToken);
    }

    #endregion Methods

}
