using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingListItems.SetShoppingListItemSequence;

/// <summary>
/// Puts the line where <paramref name="Sequence"/> says, which moves the lines it passes. Its own
/// use case rather than a field on the patch, because it is the one write that changes rows the
/// caller never named.
/// </summary>
public record SetShoppingListItemSequenceInputPort(long Sequence, long ShoppingListItemID) : IInputPort<ISetShoppingListItemSequenceOutputPort>;
