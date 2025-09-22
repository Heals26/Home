using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCartItems.GetShoppingCartItems;

public record GetShoppingCartItemsInputPort(long ShoppingCartID) : IInputPort<IGetShoppingCartItemsOutputPort>;
