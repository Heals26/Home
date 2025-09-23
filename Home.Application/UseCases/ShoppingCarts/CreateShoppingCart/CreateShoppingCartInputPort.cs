using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCarts.CreateShoppingCart;

public record CreateShoppingCartInputPort(string Name) : IInputPort<ICreateShoppingCartOutputPort>;

