using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCartItems.GetShoppingCartItem;

public record GetShoppingCartItemInputPort(long shoppingCartItem) : IInputPort<IGetShoppingCartItemOutputPort> { }
