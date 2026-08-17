using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingLists.UntickShoppingListItems;

public record UntickShoppingListItemsInputPort(long ShoppingListID)
    : IInputPort<IUntickShoppingListItemsOutputPort>;
