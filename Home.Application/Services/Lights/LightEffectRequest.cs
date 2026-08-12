namespace Home.Application.Services.Lights;

/// <summary>
/// An effect to run on one or more bulbs.
/// </summary>
/// <param name="Kind">Which effect, or <see cref="LightEffectKind.Off"/> to cancel.</param>
/// <param name="Hue">0-360. Ignored when <paramref name="Kind"/> is Off.</param>
/// <param name="Saturation">0.0-1.0.</param>
/// <param name="PeriodSeconds">How long one cycle takes.</param>
/// <param name="Cycles">How many times to repeat.</param>
/// <param name="PowerOn">Turn the bulb on first if it is off.</param>
public record LightEffectRequest(
    LightEffectKind Kind,
    double Hue,
    double Saturation,
    double PeriodSeconds,
    double Cycles,
    bool PowerOn);
