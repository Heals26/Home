using System.Text.Json.Serialization;

namespace Home.WebApi.Infrastructure.Lights;

internal class LifxColour
{

    #region Properties

    [JsonPropertyName("hue")]
    public double Hue { get; set; }

    [JsonPropertyName("kelvin")]
    public int Kelvin { get; set; }

    [JsonPropertyName("saturation")]
    public double Saturation { get; set; }

    #endregion Properties

}
