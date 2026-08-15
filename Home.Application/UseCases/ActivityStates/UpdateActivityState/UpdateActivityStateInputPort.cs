using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.ActivityStates.UpdateActivityState;

public record UpdateActivityStateInputPort(
    long ActivityStateID,
    PropertyChangeTracker<bool> IsComplete,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<int> Sequence)
    : IInputPort<IUpdateActivityStateOutputPort>;
