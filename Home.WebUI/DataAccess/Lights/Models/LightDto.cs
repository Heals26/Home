namespace Home.WebUI.DataAccess.Lights.Models;

public class LightDto
{

    #region Properties

    /// <summary>
    /// 0.0 to 1.0.
    /// </summary>
    public double Brightness { get; set; }

    /// <summary>
    /// 0 to 360.
    /// </summary>
    public double Hue { get; set; }

    /// <summary>
    /// The LIFX device ID, used to address state changes.
    /// </summary>
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// False when the bulb has not been seen recently and cannot be controlled.
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// Whether the light is currently powered on.
    /// </summary>
    public bool IsOn { get; set; }

    /// <summary>
    /// White temperature in kelvin.
    /// </summary>
    public int Kelvin { get; set; }

    /// <summary>
    /// The bulb's own name.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 0.0 to 1.0. Zero means the bulb is showing white at <see cref="Kelvin"/>.
    /// </summary>
    public double Saturation { get; set; }

    /// <summary>
    /// When Home last refreshed this state from the provider.
    /// </summary>
    public DateTime StateUpdatedUTC { get; set; }

    #endregion Properties

}
