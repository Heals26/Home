using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.LightGroups.UpdateLightGroup;

public record UpdateLightGroupInputPort(
    long LightGroupID,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<int> Sequence)
    : IInputPort<IUpdateLightGroupOutputPort>;
