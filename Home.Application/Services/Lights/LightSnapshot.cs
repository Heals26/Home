namespace Home.Application.Services.Lights;

/// <summary>
/// A light's live state as reported by the lighting provider. This is transient external data,
/// deliberately not a domain entity — the bulbs are the source of truth, not the database.
/// </summary>
public record LightSnapshot(
    string ID,
    string Label,
    string GroupID,
    string GroupName,
    string LocationName,
    bool IsConnected,
    bool IsOn,
    double Brightness,
    double Hue,
    double Saturation,
    int Kelvin);
