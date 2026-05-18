using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingLists.CreateShoppingList;

public record CreateShoppingListInputPort(string Name) : IInputPort<ICreateShoppingListOutputPort>;
