using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingListItems.GetShoppingListItem;

public record GetShoppingListItemInputPort(long ShoppingListItemID) : IInputPort<IGetShoppingListItemOutputPort>;
