using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.LightScenes.CaptureLightScene;

/// <summary>
/// Saves how the lights look right now. A null <paramref name="LightGroupID"/> captures the whole
/// household; otherwise just that group.
/// </summary>
public record CaptureLightSceneInputPort(string Name, long? LightGroupID)
    : IInputPort<ICaptureLightSceneOutputPort>;
