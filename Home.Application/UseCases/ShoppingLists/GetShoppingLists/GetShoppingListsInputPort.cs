using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingLists.GetShoppingLists;

public record GetShoppingListsInputPort() : IInputPort<IGetShoppingListsOutputPort>;
