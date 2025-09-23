using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCarts.DeleteShoppingCart;

public record DeleteShoppingCartInputPort(long ShoppingCartID) : IInputPort<IDeleteShoppingCartOutputPort>;
