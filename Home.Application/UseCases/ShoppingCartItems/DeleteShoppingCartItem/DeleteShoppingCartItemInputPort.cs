using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCartItems.DeleteShoppingCartItem;

public record DeleteShoppingCartItemInputPort(long ShoppingCartItemID)
    : IInputPort<IDeleteShoppingCartItemOutputPort>;
