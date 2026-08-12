namespace Home.Application.Services.Lights;

/// <summary>
/// The effects every bulb can run. Deliberately excludes the provider's hardware-specific ones
/// (move needs a multizone strip, morph and flame need tiles) — those would fail on a plain bulb.
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
