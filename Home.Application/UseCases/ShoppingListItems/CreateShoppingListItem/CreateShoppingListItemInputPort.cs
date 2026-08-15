using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingListItems.CreateShoppingListItem;

public record CreateShoppingListItemInputPort(
    decimal? Amount,
    decimal? Cost,
    bool InBasket,
    string Name,
    long ShoppingListID,
    long? Unit)
    : IInputPort<ICreateShoppingListItemOutputPort>;
