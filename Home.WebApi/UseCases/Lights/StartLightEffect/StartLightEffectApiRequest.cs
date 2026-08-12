using Home.Application.Services.Lights;

namespace Home.WebApi.UseCases.Lights.StartLightEffect;

/// <summary>
/// Runs a transient effect. Leave LightGroupID null to target every light in the household.
/// Effects do not persist, so the lights return to their previous state when it finishes.
/// </summary>
public record StartLightEffectApiRequest(
    long? LightGroupID,
    LightEffectKind Kind,
    double Hue,
    double Saturation,
    double PeriodSeconds,
    double Cycles);
