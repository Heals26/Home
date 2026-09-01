using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.CardSections.UpdateCardSection;

public record UpdateCardSectionInputPort(
    long CardSectionID,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<int> Sequence)
    : IInputPort<IUpdateCardSectionOutputPort>;