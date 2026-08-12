using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.LightGroups.SetLightGroupState;

/// <summary>
/// Applied to every connected light in the group in a single provider call.
/// </summary>
public class SetLightGroupStateWebAppRequest
{

    #region Properties

    /// <summary>
    /// 0.0 to 1.0.
    /// </summary>
    public PropertyChangeTracker<double> Brightness { get; set; }

    /// <summary>
    /// 0 to 360.
    /// </summary>
    public PropertyChangeTracker<double> Hue { get; set; }

    /// <summary>
    /// Whether the lights should be powered on.
    /// </summary>
    public PropertyChangeTracker<bool> IsOn { get; set; }

    /// <summary>
    /// White temperature in kelvin. Setting this drives saturation to zero.
    /// </summary>
    public PropertyChangeTracker<int> Kelvin { get; set; }

    /// <summary>
    /// 0.0 to 1.0.
    /// </summary>
    public PropertyChangeTracker<double> Saturation { get; set; }

    #endregion Properties

}
