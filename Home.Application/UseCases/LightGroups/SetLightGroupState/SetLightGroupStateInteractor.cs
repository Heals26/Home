using CleanArchitecture.Mediator;
using Home.Application.Services.Lights;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.LightGroups.SetLightGroupState;

/// <summary>
/// Applies one change to every connected bulb in a group. The provider is asked once for the whole
/// set rather than once per bulb, so a ten-light room costs a single call.
/// </summary>
internal class SetLightGroupStateInteractor
    : IInteractor<SetLightGroupStateInputPort, ISetLightGroupStateOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SetLightGroupStateInputPort inputPort,
        ISetLightGroupStateOutputPort outputPort,
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

        var _Group = _PersistenceContext.GetEntities<LightGroup>()
            .Where(g => g.LightGroupID == inputPort.LightGroupID
                && g.Location.Household.HouseholdID == _Household.HouseholdID)
            .Select(g => new { Group = g, g.Lights })
            .SingleOrDefault()
            ?.Group;

        if (_Group == null)
        {
            await outputPort.PresentLightGroupNotFoundAsync(inputPort.LightGroupID, cancellationToken);
            return;
        }

        // An offline bulb cannot act on the command, and including it only risks the provider
        // rejecting the whole selector.
        var _Lights = _Group.Lights.Where(l => l.IsConnected).ToList();

        if (_Lights.Count == 0)
        {
            await outputPort.PresentLightGroupStateSetAsync(cancellationToken);
            return;
        }

        var _Result = await _LightService.SetGroupStateAsync(
            [.. _Lights.Select(l => l.ID)], _Change, cancellationToken);

        if (_Result == LightCommandResult.Unavailable)
        {
            await outputPort.PresentLightsUnavailableAsync(cancellationToken);
            return;
        }

        var _Now = serviceFactory.GetService<TimeProvider>().GetUtcNow().UtcDateTime;

        foreach (var _Light in _Lights)
            ApplyChange(_Light, _Change, _Now);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentLightGroupStateSetAsync(cancellationToken);
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

        if (change.Kelvin.HasBeenSet)
        {
            light.Kelvin = change.Kelvin.Value;
            light.Saturation = 0d;
        }

        light.StateUpdatedUTC = nowUTC;
    }

    #endregion Methods

}
