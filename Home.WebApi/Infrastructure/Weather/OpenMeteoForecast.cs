using System.Text.Json.Serialization;

namespace Home.WebApi.Infrastructure.Weather;

/// <summary>
/// The wire shape of an Open-Meteo forecast response.
/// </summary>
internal class OpenMeteoForecast
{

    #region Properties

    [JsonPropertyName("current")]
    public OpenMeteoCurrent Current { get; set; }

    [JsonPropertyName("daily")]
    public OpenMeteoDaily Daily { get; set; }

    #endregion Properties

}
