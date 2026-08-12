namespace Home.Application.Services.Lights;

/// <summary>
/// A light's live state as reported by the lighting provider. This is transient external data —
/// the bulbs are the source of truth for state, and Home caches it onto
/// <see cref="Home.Domain.Entities.Light"/> during a sync.
/// </summary>
public record LightSnapshot(
    string ID,
    string Label,
    string GroupID,
    string GroupName,
    string LocationID,
    string LocationName,
    bool IsConnected,
    bool IsOn,
    double Brightness,
    double Hue,
    double Saturation,
    int Kelvin,
    LightCapabilities Capabilities);
