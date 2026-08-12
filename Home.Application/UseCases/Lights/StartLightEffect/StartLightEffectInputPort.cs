using CleanArchitecture.Mediator;
using Home.Application.Services.Lights;

namespace Home.Application.UseCases.Lights.StartLightEffect;

/// <summary>
/// Runs an effect on a group. A null <paramref name="LightGroupID"/> targets every light in the
/// household, which is what "flash the house" looks like.
/// </summary>
public record StartLightEffectInputPort(
    long? LightGroupID,
    LightEffectKind Kind,
    double Hue,
    double Saturation,
    double PeriodSeconds,
    double Cycles)
    : IInputPort<IStartLightEffectOutputPort>;
