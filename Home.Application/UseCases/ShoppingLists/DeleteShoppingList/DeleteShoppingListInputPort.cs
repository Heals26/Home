using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingLists.DeleteShoppingList;

public record DeleteShoppingListInputPort(long ShoppingListID) : IInputPort<IDeleteShoppingListOutputPort>;
