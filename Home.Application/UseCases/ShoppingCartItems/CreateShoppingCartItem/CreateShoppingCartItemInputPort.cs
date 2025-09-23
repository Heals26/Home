using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCartItems.CreateShoppingCartItem;

public record CreateShoppingCartItemInputPort(
    int Cost,
    bool InBasket,
    string Name,
    int Quantity,
    long ShoppingCartID)
    : IInputPort<ICreateShoppingCartItemOutputPort>;
