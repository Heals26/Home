using System.Text.Json.Serialization;

namespace Home.WebApi.Infrastructure.Lights;

/// <summary>
/// Not every field appears on every device — switches omit the colour flags, plain bulbs omit the
/// matrix and relay ones — so all of these default to false or zero when absent.
/// </summary>
internal class LifxProductCapabilities
{

    #region Properties

    [JsonPropertyName("has_chain")]
    public bool HasChain { get; set; }

    [JsonPropertyName("has_color")]
    public bool HasColour { get; set; }

    [JsonPropertyName("has_matrix")]
    public bool HasMatrix { get; set; }

    [JsonPropertyName("has_multizone")]
    public bool HasMultizone { get; set; }

    [JsonPropertyName("has_variable_color_temp")]
    public bool HasVariableColourTemp { get; set; }

    [JsonPropertyName("max_kelvin")]
    public int MaxKelvin { get; set; }

    [JsonPropertyName("min_kelvin")]
    public int MinKelvin { get; set; }

    #endregion Properties

}
