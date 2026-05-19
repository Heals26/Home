using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingListItems.CreateShoppingListItem;

public record CreateShoppingListItemInputPort(
    decimal? Cost,
    bool InBasket,
    string Name,
    decimal? Quantity,
    long ShoppingListID,
    decimal? Volume,
    decimal? Weight)
    : IInputPort<ICreateShoppingListItemOutputPort>;
