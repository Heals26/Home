using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.Tags.UpdateTag;

public record UpdateTagInputPort(
    long TagID,
    PropertyChangeTracker<string> Colour,
    PropertyChangeTracker<string> Name)
    : IInputPort<IUpdateTagOutputPort>;
