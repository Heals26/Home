using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCarts.GetShoppingCarts;

public record GetShoppingCartsInputPort : IInputPort<IGetShoppingCartsOutputPort> { }
