using System.Text.Json.Serialization;

namespace Home.WebApi.Infrastructure.Lights;

/// <summary>
/// The wire shape of a light in a LIFX list response. Kept internal and separate from
/// <see cref="Home.Application.Services.Lights.LightSnapshot"/> so a change at LIFX's end
/// doesn't leak into the use cases.
/// </summary>
internal class LifxLight
{

    #region Properties

    [JsonPropertyName("brightness")]
    public double Brightness { get; set; }

    [JsonPropertyName("color")]
    public LifxColour Colour { get; set; }

    [JsonPropertyName("connected")]
    public bool Connected { get; set; }

    [JsonPropertyName("group")]
    public LifxNamedReference Group { get; set; }

    [JsonPropertyName("id")]
    public string ID { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("location")]
    public LifxNamedReference Location { get; set; }

    [JsonPropertyName("product")]
    public LifxProduct Product { get; set; }

    [JsonPropertyName("power")]
    public string Power { get; set; }

    #endregion Properties

}
