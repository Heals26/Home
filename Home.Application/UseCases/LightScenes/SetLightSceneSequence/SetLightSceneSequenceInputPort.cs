using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.LightScenes.SetLightSceneSequence;

public record SetLightSceneSequenceInputPort(long LightSceneID, int Sequence) : IInputPort<ISetLightSceneSequenceOutputPort>;
