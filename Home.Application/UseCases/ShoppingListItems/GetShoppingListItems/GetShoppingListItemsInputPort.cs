using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingListItems.GetShoppingListItems;

public record GetShoppingListItemsInputPort(long ShoppingListID) : IInputPort<IGetShoppingListItemsOutputPort>;
