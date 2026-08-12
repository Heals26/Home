using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.LightScenes.DeleteLightScene;

public record DeleteLightSceneInputPort(long LightSceneID) : IInputPort<IDeleteLightSceneOutputPort>;
