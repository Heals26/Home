namespace Home.WebUI.DataAccess.Lights.Models;

/// <summary>
/// Mirrors the API's effect kinds. Values must match — the enum crosses the wire as a number.
/// </summary>
public enum LightEffectKind
{

    /// <summary>Cancels whatever effect is running.</summary>
    Off = 0,

    /// <summary>Smooth fade in and out of a colour.</summary>
    Breathe = 1,

    /// <summary>Hard blink between the current colour and another.</summary>
    Pulse = 2,

}
