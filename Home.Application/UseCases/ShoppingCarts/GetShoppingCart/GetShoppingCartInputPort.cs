using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCarts.GetShoppingCart;

public record GetShoppingCartInputPort(long ShoppingCartID) : IInputPort<IGetShoppingCartOutputPort>;
