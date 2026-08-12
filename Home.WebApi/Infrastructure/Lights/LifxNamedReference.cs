using System.Text.Json.Serialization;

namespace Home.WebApi.Infrastructure.Lights;

/// <summary>
/// LIFX returns groups and locations as the same { id, name } pair.
/// </summary>
internal class LifxNamedReference
{

    #region Properties

    [JsonPropertyName("id")]
    public string ID { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    #endregion Properties

}
