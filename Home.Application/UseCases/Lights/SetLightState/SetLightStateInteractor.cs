using CleanArchitecture.Mediator;
using Home.Application.Services.Lights;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

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
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
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

        var _Household = _AuthorisationService.GetHousehold();

        var _Light = _PersistenceContext.GetEntities<Light>()
            .Where(l => l.ID == inputPort.LightID
                && l.Group.Location.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_Light == null)
        {
            await outputPort.PresentLightNotFoundAsync(inputPort.LightID, cancellationToken);
            return;
        }

        var _Result = await _LightService.SetStateAsync(inputPort.LightID, _Change, cancellationToken);

        if (_Result == LightCommandResult.LightNotFound)
        {
            await outputPort.PresentLightNotFoundAsync(inputPort.LightID, cancellationToken);
            return;
        }

        if (_Result == LightCommandResult.Unavailable)
        {
            await outputPort.PresentLightsUnavailableAsync(cancellationToken);
            return;
        }

        // The provider took it, so Home's cached copy is now correct without another read.
        ApplyChange(_Light, _Change, serviceFactory.GetService<TimeProvider>().GetUtcNow().UtcDateTime);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightStateSetAsync(cancellationToken);
    }

    private static void ApplyChange(Light light, LightStateChange change, DateTime nowUTC)
    {
        if (change.IsOn.HasBeenSet)
            light.IsOn = change.IsOn.Value;

        if (change.Brightness.HasBeenSet)
            light.Brightness = change.Brightness.Value;

        if (change.Hue.HasBeenSet)
            light.Hue = change.Hue.Value;

        if (change.Saturation.HasBeenSet)
            light.Saturation = change.Saturation.Value;

        // Kelvin and hue are mutually exclusive on the wire: asking for a white temperature drives
        // saturation to zero, so Home's copy has to reflect that or the swatch lies.
        if (change.Kelvin.HasBeenSet)
        {
            light.Kelvin = change.Kelvin.Value;
            light.Saturation = 0d;
        }

        light.StateUpdatedUTC = nowUTC;
    }

    #endregion Methods

}
