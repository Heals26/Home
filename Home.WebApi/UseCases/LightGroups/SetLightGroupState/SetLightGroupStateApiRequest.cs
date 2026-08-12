using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.LightGroups.SetLightGroupState;

/// <summary>
/// A partial state change applied to every connected light in the group in a single provider call.
/// </summary>
public record SetLightGroupStateApiRequest(
    PropertyChangeTracker<bool> IsOn,
    PropertyChangeTracker<double> Brightness,
    PropertyChangeTracker<double> Hue,
    PropertyChangeTracker<double> Saturation,
    PropertyChangeTracker<int> Kelvin);
