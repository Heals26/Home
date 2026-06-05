using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ActivityStates.GetActivityStates;

public record GetActivityStatesInputPort() : IInputPort<IGetActivityStatesOutputPort>;
