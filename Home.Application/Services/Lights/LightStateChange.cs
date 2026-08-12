using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.Services.Lights;

/// <summary>
/// A partial change to a light's state. Anything left unset is not sent to the provider, so a
/// brightness change never disturbs the colour and vice versa.
/// </summary>
public record LightStateChange(
    PropertyChangeTracker<bool> IsOn,
    PropertyChangeTracker<double> Brightness,
    PropertyChangeTracker<double> Hue,
    PropertyChangeTracker<double> Saturation,
    PropertyChangeTracker<int> Kelvin)
{

    #region Properties

    /// <summary>
    /// True when nothing at all was set, which the provider would reject as an empty request.
    /// </summary>
    public bool IsEmpty
        => !this.IsOn.HasBeenSet
        && !this.Brightness.HasBeenSet
        && !this.Hue.HasBeenSet
        && !this.Saturation.HasBeenSet
        && !this.Kelvin.HasBeenSet;

    #endregion Properties

}
