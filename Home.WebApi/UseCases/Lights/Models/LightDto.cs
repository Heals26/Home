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


    /// <summary>
    /// True when the bulb can show colour. White-only bulbs get no colour picker.
    /// </summary>
    public bool HasColour { get; set; }

    /// <summary>
    /// True when the hardware is a panel of individually addressable zones. Home still drives it
    /// as one light; the UI says so rather than implying per-zone control it does not have.
    /// </summary>
    public bool HasMatrix { get; set; }

    /// <summary>
    /// True when the hardware is a strip of individually addressable zones.
    /// </summary>
    public bool HasMultizone { get; set; }

    /// <summary>
    /// True when the bulb can change white temperature.
    /// </summary>
    public bool HasVariableColourTemp { get; set; }

    /// <summary>
    /// The bulb's white-temperature range in kelvin. Zero when it reported none.
    /// </summary>
    public int MaxKelvin { get; set; }

    public int MinKelvin { get; set; }

    /// <summary>
    /// The hardware's own name, e.g. "LIFX A19".
    /// </summary>
    public string ProductName { get; set; }

    #endregion Properties

}
