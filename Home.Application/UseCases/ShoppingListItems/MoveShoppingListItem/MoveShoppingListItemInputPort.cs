using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingListItems.MoveShoppingListItem;

/// <summary>
/// Moves one item onto a different list. Both the item and the list are checked against the
/// household, because a move touches two lists and either of them could belong to someone else.
/// </summary>
public record MoveShoppingListItemInputPort(long ShoppingListID, long ShoppingListItemID)
    : IInputPort<IMoveShoppingListItemOutputPort>;
