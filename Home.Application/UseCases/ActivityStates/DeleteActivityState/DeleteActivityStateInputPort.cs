using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ActivityStates.DeleteActivityState;

/// <summary>
/// MoveCardsToStateID is the column every card in the deleted one is dragged to, so removing a
/// column can never orphan a card.
/// </summary>
public record DeleteActivityStateInputPort(
    long ActivityStateID,
    long MoveCardsToStateID)
    : IInputPort<IDeleteActivityStateOutputPort>;
