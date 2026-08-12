using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.LightScenes.ApplyLightScene;

public record ApplyLightSceneInputPort(long LightSceneID) : IInputPort<IApplyLightSceneOutputPort>;
