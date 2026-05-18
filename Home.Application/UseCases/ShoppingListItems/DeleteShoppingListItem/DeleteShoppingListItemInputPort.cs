using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingListItems.DeleteShoppingListItem;

public record DeleteShoppingListItemInputPort(long ShoppingListItemID) : IInputPort<IDeleteShoppingListItemOutputPort>;
