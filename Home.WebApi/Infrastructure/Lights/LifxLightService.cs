// Home.WebApi has nullable disabled project-wide, but ILightService's contract is nullable-aware
// (a null light list means "provider unreachable"). Opting this file in keeps that meaning.
#nullable enable

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

    public Task<LightCommandResult> SetStateAsync(
        string lightID,
        LightStateChange change,
        CancellationToken cancellationToken)
        => this.SetGroupStateAsync([lightID], change, cancellationToken);

    /// <summary>
    /// LIFX accepts up to <see cref="LightValues.MaxSelectorsPerRequest"/> comma-separated
    /// selectors in one call, so a whole room costs one request rather than one per bulb. Larger
    /// sets are chunked; a partial failure across chunks is reported as a failure overall.
    /// </summary>
    public async Task<LightCommandResult> SetGroupStateAsync(
        IReadOnlyCollection<string> lightIDs,
        LightStateChange change,
        CancellationToken cancellationToken)
    {
        var _Body = BuildStateBody(change);

        if (_Body.Count == 0 || lightIDs.Count == 0)
            return LightCommandResult.Applied;

        var _Payload = JsonSerializer.Serialize(_Body);
        var _Result = LightCommandResult.Applied;

        foreach (var _Chunk in Chunk(lightIDs, LightValues.MaxSelectorsPerRequest))
        {
            var _ChunkResult = await this.SendStateAsync(_Chunk, _Payload, cancellationToken);

            // Unavailable outranks NotFound: one bulb missing matters less than the hub being down.
            if (_ChunkResult == LightCommandResult.Unavailable)
                return LightCommandResult.Unavailable;

            if (_ChunkResult == LightCommandResult.LightNotFound)
                _Result = LightCommandResult.LightNotFound;
        }

        return _Result;
    }

    public async Task<LightCommandResult> StartEffectAsync(
        IReadOnlyCollection<string> lightIDs,
        LightEffectRequest effect,
        CancellationToken cancellationToken)
    {
        if (lightIDs.Count == 0)
            return LightCommandResult.Applied;

        // "off" takes a different body from the running effects, so build them separately.
        var _Path = effect.Kind switch
        {
            LightEffectKind.Breathe => "breathe",
            LightEffectKind.Pulse => "pulse",
            _ => "off"
        };

        var _Body = effect.Kind == LightEffectKind.Off
            ? new Dictionary<string, object> { ["power_off"] = false }
            : new Dictionary<string, object>
            {
                ["color"] = Format("hue", Math.Clamp(effect.Hue, 0d, 360d))
                    + ' ' + Format("saturation", Math.Clamp(effect.Saturation, 0d, 1d)),
                ["period"] = Math.Clamp(effect.PeriodSeconds, 0.1d, 3600d),
                ["cycles"] = Math.Clamp(effect.Cycles, 1d, 1000d),
                ["power_on"] = effect.PowerOn,
                // Without this the bulb snaps back to its previous colour when the effect ends,
                // which is what makes an effect feel like a notification rather than a state change.
                ["persist"] = false
            };

        var _Payload = JsonSerializer.Serialize(_Body);
        var _Result = LightCommandResult.Applied;

        foreach (var _Chunk in Chunk(lightIDs, LightValues.MaxSelectorsPerRequest))
        {
            var _ChunkResult = await this.SendEffectAsync(_Chunk, _Path, _Payload, cancellationToken);

            if (_ChunkResult == LightCommandResult.Unavailable)
                return LightCommandResult.Unavailable;

            if (_ChunkResult == LightCommandResult.LightNotFound)
                _Result = LightCommandResult.LightNotFound;
        }

        return _Result;
    }

    private async Task<LightCommandResult> SendEffectAsync(
        IReadOnlyList<string> lightIDs,
        string effectPath,
        string payload,
        CancellationToken cancellationToken)
    {
        var _Selector = string.Join(',', lightIDs.Select(id => $"id:{Uri.EscapeDataString(id)}"));

        try
        {
            using var _Content = new StringContent(payload, Encoding.UTF8, "application/json");
            using var _Response = await httpClient.PostAsync(
                $"lights/{_Selector}/effects/{effectPath}", _Content, cancellationToken);

            if (_Response.StatusCode is HttpStatusCode.NotFound)
                return LightCommandResult.LightNotFound;

            if (!_Response.IsSuccessStatusCode)
            {
                logger.LogWarning("LIFX returned {StatusCode} starting the {Effect} effect.",
                    _Response.StatusCode, effectPath);

                return LightCommandResult.Unavailable;
            }

            return LightCommandResult.Applied;
        }
        catch (Exception _Exception) when (_Exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(_Exception, "Could not reach LIFX to start the {Effect} effect.", effectPath);
            return LightCommandResult.Unavailable;
        }
    }

    private async Task<LightCommandResult> SendStateAsync(
        IReadOnlyList<string> lightIDs,
        string payload,
        CancellationToken cancellationToken)
    {
        var _Selector = string.Join(',', lightIDs.Select(id => $"id:{Uri.EscapeDataString(id)}"));

        try
        {
            using var _Content = new StringContent(payload, Encoding.UTF8, "application/json");
            using var _Response = await httpClient.PutAsync($"lights/{_Selector}/state", _Content, cancellationToken);

            if (_Response.StatusCode is HttpStatusCode.NotFound)
                return LightCommandResult.LightNotFound;

            // 429 means we have run out of the 120-per-minute budget. Treated as unavailable so the
            // caller backs off rather than retrying into the same wall.
            if (_Response.StatusCode is HttpStatusCode.TooManyRequests)
            {
                logger.LogWarning("LIFX rate limit hit; {Remaining} requests left, resets at {Reset}.",
                    HeaderValue(_Response, "X-RateLimit-Remaining"),
                    HeaderValue(_Response, "X-RateLimit-Reset"));

                return LightCommandResult.Unavailable;
            }

            if (!_Response.IsSuccessStatusCode)
            {
                logger.LogWarning("LIFX returned {StatusCode} setting state on {LightCount} light(s).",
                    _Response.StatusCode, lightIDs.Count);

                return LightCommandResult.Unavailable;
            }

            return LightCommandResult.Applied;
        }
        catch (Exception _Exception) when (_Exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(_Exception, "Could not reach LIFX to set state on {LightCount} light(s).", lightIDs.Count);
            return LightCommandResult.Unavailable;
        }
    }

    private static IEnumerable<IReadOnlyList<string>> Chunk(IReadOnlyCollection<string> source, int size)
    {
        var _Batch = new List<string>(size);

        foreach (var _Item in source)
        {
            _Batch.Add(_Item);

            if (_Batch.Count != size)
                continue;

            yield return _Batch;
            _Batch = new List<string>(size);
        }

        if (_Batch.Count > 0)
            yield return _Batch;
    }

    private static string HeaderValue(HttpResponseMessage response, string name)
        => response.Headers.TryGetValues(name, out var _Values) ? string.Join(',', _Values) : "unknown";

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
            light.Location?.ID ?? string.Empty,
            light.Location?.Name ?? "Home",
            light.Connected,
            string.Equals(light.Power, "on", StringComparison.OrdinalIgnoreCase),
            light.Brightness,
            light.Colour?.Hue ?? 0d,
            light.Colour?.Saturation ?? 0d,
            light.Colour?.Kelvin ?? 0);

    #endregion Methods

}
