using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.Lights.SetLightState;

/// <summary>
/// A partial state change. Omit a property to leave it alone — sending only Brightness will not
/// disturb the light's colour.
/// </summary>
public record SetLightStateApiRequest(
    PropertyChangeTracker<bool> IsOn,
    PropertyChangeTracker<double> Brightness,
    PropertyChangeTracker<double> Hue,
    PropertyChangeTracker<double> Saturation,
    PropertyChangeTracker<int> Kelvin);
