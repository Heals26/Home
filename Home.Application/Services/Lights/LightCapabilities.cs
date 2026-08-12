namespace Home.Application.Services.Lights;

/// <summary>
/// What a bulb's hardware supports. Home hides controls a bulb cannot honour rather than sending
/// commands it will ignore.
/// </summary>
/// <param name="MinKelvin">Zero when the hardware reported no white range.</param>
public record LightCapabilities(
    bool HasColour,
    bool HasVariableColourTemp,
    bool HasMultizone,
    bool HasMatrix,
    int MinKelvin,
    int MaxKelvin,
    string ProductName);
