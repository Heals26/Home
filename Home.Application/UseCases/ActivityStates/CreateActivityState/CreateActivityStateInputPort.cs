using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ActivityStates.CreateActivityState;

public record CreateActivityStateInputPort(
    string Name,
    bool IsComplete)
    : IInputPort<ICreateActivityStateOutputPort>;
