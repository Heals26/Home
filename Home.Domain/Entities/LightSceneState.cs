namespace Home.Domain.Entities;

/// <summary>
/// What one light should look like when its scene is recalled. A light that was off at capture
/// time is stored as off, so recalling a scene turns things off as well as on.
/// </summary>
public class LightSceneState
{

    #region Properties

    public long LightSceneStateID { get; set; }

    public double Brightness { get; set; }
    public double Hue { get; set; }
    public bool IsOn { get; set; }
    public int Kelvin { get; set; }
    public double Saturation { get; set; }

    public Light Light { get; set; } = null!;
    public LightScene Scene { get; set; } = null!;

    #endregion Properties

}
