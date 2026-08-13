using Home.WebUI.DataAccess.Lights.Models;

namespace Home.WebUI.DataAccess.Lights.StartLightEffect;

/// <summary>
/// Runs a transient effect. Leave LightGroupID null to target every light in the household.
/// Effects do not persist, so the lights return to their previous state when it finishes.
/// </summary>
public class StartLightEffectWebAppRequest
{

    #region Properties

    /// <summary>
    /// How many times the effect repeats.
    /// </summary>
    public double Cycles { get; set; }

    /// <summary>
    /// 0 to 360.
    /// </summary>
    public double Hue { get; set; }

    /// <summary>
    /// The effect to run, or Off to cancel.
    /// </summary>
    public LightEffectKind Kind { get; set; }

    /// <summary>
    /// Leave null to target every light in the household.
    /// </summary>
    public long? LightGroupID { get; set; }

    /// <summary>
    /// Seconds per cycle.
    /// </summary>
    public double PeriodSeconds { get; set; }

    /// <summary>
    /// 0.0 to 1.0.
    /// </summary>
    public double Saturation { get; set; }

    #endregion Properties

}
