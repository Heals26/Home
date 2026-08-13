namespace Home.Domain.Entities;

public class Light
{

    #region Properties

    public long LightID { get; set; }
    public string ID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public LightGroup Group { get; set; } = null!;

    #endregion Properties

    #region Capabilities

    // What the hardware can actually do, read from the provider on sync. The UI hides controls a
    // bulb cannot honour, and effects are offered only where they will work.

    public bool HasColour { get; set; }
    public bool HasMatrix { get; set; }
    public bool HasMultizone { get; set; }
    public bool HasVariableColourTemp { get; set; }

    /// <summary>
    /// The bulb's white-temperature range. Zero on hardware that reported neither.
    /// </summary>
    public int MaxKelvin { get; set; }

    public int MinKelvin { get; set; }

    /// <summary>
    /// The hardware's own name, e.g. "LIFX A19". Shown so a user can tell two bulbs apart.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    #endregion Capabilities

    #region Cached State

    // Last state read from the provider. Cached so opening the Lights page costs nothing and the
    // tablet has something to draw before the first refresh comes back.

    public double Brightness { get; set; }
    public double Hue { get; set; }
    public bool IsConnected { get; set; }
    public bool IsOn { get; set; }
    public int Kelvin { get; set; }
    public double Saturation { get; set; }
    public DateTime StateUpdatedUTC { get; set; }

    #endregion Cached State

}
