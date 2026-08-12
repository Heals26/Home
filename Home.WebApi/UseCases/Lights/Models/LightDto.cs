namespace Home.WebApi.UseCases.Lights.Models;

public class LightDto
{

    #region Properties

    /// <summary>
    /// 0.0 to 1.0. Meaningful even when the light is off — it is the level it will return to.
    /// </summary>
    public double Brightness { get; set; }

    /// <summary>
    /// 0 to 360.
    /// </summary>
    public double Hue { get; set; }

    /// <summary>
    /// The LIFX device ID. Stable, and what state changes are addressed to.
    /// </summary>
    public string ID { get; set; }

    /// <summary>
    /// False when the bulb has not been seen recently — it cannot be controlled.
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// Whether the light is currently powered on.
    /// </summary>
    public bool IsOn { get; set; }

    /// <summary>
    /// White temperature in kelvin, 1500 to 9000.
    /// </summary>
    public int Kelvin { get; set; }

    /// <summary>
    /// The bulb's own name, e.g. "Bedside Left".
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// 0.0 to 1.0. Zero means white, in which case Kelvin is what matters.
    /// </summary>
    public double Saturation { get; set; }

    /// <summary>
    /// When Home last refreshed this state from the provider. Lets the UI show how stale it is.
    /// </summary>
    public DateTime StateUpdatedUTC { get; set; }

    #endregion Properties

}
