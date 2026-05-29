using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.ActivityContents.UpdateActivityContent;

public record UpdateActivityContentInputPort(
    long ActivityContentID,
    PropertyChangeTracker<string> Content,
    PropertyChangeTracker<int> Sequence)
    : IInputPort<IUpdateActivityContentOutputPort>;
