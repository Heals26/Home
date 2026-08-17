using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingLists.DeleteTickedShoppingListItems;

public record DeleteTickedShoppingListItemsInputPort(long ShoppingListID)
    : IInputPort<IDeleteTickedShoppingListItemsOutputPort>;
