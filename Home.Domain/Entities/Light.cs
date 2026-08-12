namespace Home.Domain.Entities;

public class Light
{

    #region Properties

    public long LightID { get; set; }
    public string ID { get; set; }
    public string Name { get; set; }

    public LightGroup Group { get; set; }

    #endregion Properties

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
