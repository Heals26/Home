using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Services.EntityLogic.Lights;
using Home.Application.Services.Lights;
using Home.Domain.Entities;

namespace Home.Application.Infrastructure.Lights;

public class LightSceneLogic(ILightService lightService, TimeProvider timeProvider) : ILightSceneLogic
{

    #region Methods

    /// <summary>
    /// Lights heading to identical settings are sent together, so a scene where a whole room goes
    /// to the same warm dim costs one provider call rather than one per bulb. Offline bulbs are
    /// skipped. The caller owns saving.
    /// </summary>
    async Task<LightCommandResult> ILightSceneLogic.ApplyAsync(LightScene scene, CancellationToken cancellationToken)
    {
        var _States = scene.States.Where(s => s.Light.IsConnected).ToList();

        if (_States.Count == 0)
            return LightCommandResult.Applied;

        foreach (var _Batch in _States.GroupBy(s => (s.IsOn, s.Brightness, s.Hue, s.Saturation, s.Kelvin)))
        {
            var _Result = await lightService.SetGroupStateAsync(
                [.. _Batch.Select(s => s.Light.ID)], BuildChange(_Batch.Key), cancellationToken);

            if (_Result == LightCommandResult.Unavailable)
                return LightCommandResult.Unavailable;

            foreach (var _State in _Batch)
                ApplyToCache(_State, timeProvider.GetUtcNow().UtcDateTime);
        }

        return LightCommandResult.Applied;
    }

    /// <summary>
    /// A bulb saved as off only needs the power command — colour on a light about to go dark says
    /// nothing and costs the same.
    /// </summary>
    private static LightStateChange BuildChange(
        (bool IsOn, double Brightness, double Hue, double Saturation, int Kelvin) state)
        => state.IsOn
            ? new LightStateChange(
                new PropertyChangeTracker<bool>(true),
                new PropertyChangeTracker<double>(state.Brightness),
                state.Saturation > 0 ? new PropertyChangeTracker<double>(state.Hue) : new(),
                new PropertyChangeTracker<double>(state.Saturation),
                state.Saturation <= 0 ? new PropertyChangeTracker<int>(state.Kelvin) : new())
            : new LightStateChange(new PropertyChangeTracker<bool>(false), new(), new(), new(), new());

    private static void ApplyToCache(LightSceneState state, DateTime nowUTC)
    {
        state.Light.IsOn = state.IsOn;

        if (state.IsOn)
        {
            state.Light.Brightness = state.Brightness;
            state.Light.Hue = state.Hue;
            state.Light.Saturation = state.Saturation;
            state.Light.Kelvin = state.Kelvin;
        }

        state.Light.StateUpdatedUTC = nowUTC;
    }

    #endregion Methods

}
