using System.Text.Json.Serialization;

namespace Home.WebApi.Infrastructure.Lights;

/// <summary>
/// The hardware behind a light. Its capabilities decide which controls Home offers — a white-only
/// bulb should never be shown a colour picker.
/// </summary>
internal class LifxProduct
{

    #region Properties

    [JsonPropertyName("capabilities")]
    public LifxProductCapabilities Capabilities { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    #endregion Properties

}
