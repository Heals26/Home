using Home.Application.Services.Lights;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Home.WebApi.Infrastructure.Lights;

/// <summary>
/// Drives LIFX bulbs through the LIFX cloud HTTP API (https://api.lifx.com/v1). Every call
/// round-trips to the internet, so the hub being unreachable is an expected outcome rather than
/// an exception — the whole surface returns a result instead of throwing.
/// </summary>
internal class LifxLightService(HttpClient httpClient, ILogger<LifxLightService> logger) : ILightService
{

    #region Fields

    private static readonly JsonSerializerOptions s_JsonOptions = new() { PropertyNameCaseInsensitive = true };

    #endregion Fields

    #region Methods

    public async Task<IReadOnlyList<LightSnapshot>?> GetLightsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var _Response = await httpClient.GetAsync("lights/all", cancellationToken);

            if (!_Response.IsSuccessStatusCode)
            {
                logger.LogWarning("LIFX returned {StatusCode} listing lights.", _Response.StatusCode);
                return null;
            }

            var _Payload = await _Response.Content.ReadAsStringAsync(cancellationToken);
            var _Lights = JsonSerializer.Deserialize<List<LifxLight>>(_Payload, s_JsonOptions);

            return _Lights == null ? null : [.. _Lights.Select(ToSnapshot)];
        }
        catch (Exception _Exception) when (_Exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(_Exception, "Could not reach LIFX to list lights.");
            return null;
        }
    }

    public async Task<LightCommandResult> SetStateAsync(
        string lightID,
        LightStateChange change,
        CancellationToken cancellationToken)
    {
        var _Body = BuildStateBody(change);

        if (_Body.Count == 0)
            return LightCommandResult.Applied;

        try
        {
            using var _Content = new StringContent(
                JsonSerializer.Serialize(_Body), Encoding.UTF8, "application/json");

            using var _Response = await httpClient.PutAsync(
                $"lights/id:{Uri.EscapeDataString(lightID)}/state", _Content, cancellationToken);

            if (_Response.StatusCode is HttpStatusCode.NotFound)
                return LightCommandResult.LightNotFound;

            if (!_Response.IsSuccessStatusCode)
            {
                logger.LogWarning("LIFX returned {StatusCode} setting state on {LightID}.",
                    _Response.StatusCode, lightID);
                return LightCommandResult.Unavailable;
            }

            return LightCommandResult.Applied;
        }
        catch (Exception _Exception) when (_Exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(_Exception, "Could not reach LIFX to set state on {LightID}.", lightID);
            return LightCommandResult.Unavailable;
        }
    }

    /// <summary>
    /// Only the properties that were actually set make it into the request, so adjusting
    /// brightness never resets the colour.
    /// </summary>
    private static Dictionary<string, object> BuildStateBody(LightStateChange change)
    {
        var _Body = new Dictionary<string, object>();

        if (change.IsOn.HasBeenSet)
            _Body["power"] = change.IsOn.Value ? "on" : "off";

        if (change.Brightness.HasBeenSet)
            _Body["brightness"] = Math.Clamp(change.Brightness.Value, 0d, 1d);

        var _Colour = BuildColour(change);

        if (_Colour != null)
            _Body["color"] = _Colour;

        return _Body;
    }

    /// <summary>
    /// LIFX takes colour as a single space-separated string such as "hue:120 saturation:1.0".
    /// Kelvin and hue are mutually exclusive on the wire: asking for a white temperature means
    /// saturation must drop to zero, or the bulb stays coloured.
    /// </summary>
    private static string? BuildColour(LightStateChange change)
    {
        var _Parts = new List<string>();

        if (change.Kelvin.HasBeenSet)
        {
            _Parts.Add(Format("saturation", 0d));
            _Parts.Add($"kelvin:{Math.Clamp(change.Kelvin.Value, 1500, 9000)}");
            return string.Join(' ', _Parts);
        }

        if (change.Hue.HasBeenSet)
            _Parts.Add(Format("hue", Math.Clamp(change.Hue.Value, 0d, 360d)));

        if (change.Saturation.HasBeenSet)
            _Parts.Add(Format("saturation", Math.Clamp(change.Saturation.Value, 0d, 1d)));

        return _Parts.Count == 0 ? null : string.Join(' ', _Parts);
    }

    // Invariant culture matters here: a machine set to a comma decimal separator would otherwise
    // send "hue:120,5" and LIFX would reject the whole request.
    private static string Format(string name, double value)
        => $"{name}:{value.ToString("0.###", CultureInfo.InvariantCulture)}";

    private static LightSnapshot ToSnapshot(LifxLight light)
        => new(
            light.ID ?? string.Empty,
            light.Label ?? "Unnamed light",
            light.Group?.ID ?? string.Empty,
            light.Group?.Name ?? "Ungrouped",
            light.Location?.Name ?? string.Empty,
            light.Connected,
            string.Equals(light.Power, "on", StringComparison.OrdinalIgnoreCase),
            light.Brightness,
            light.Colour?.Hue ?? 0d,
            light.Colour?.Saturation ?? 0d,
            light.Colour?.Kelvin ?? 0);

    #endregion Methods

}
