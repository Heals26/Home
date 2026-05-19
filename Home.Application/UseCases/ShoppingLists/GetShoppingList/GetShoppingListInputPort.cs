using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingLists.GetShoppingList;

public record GetShoppingListInputPort(long ShoppingListID) : IInputPort<IGetShoppingListOutputPort>;
