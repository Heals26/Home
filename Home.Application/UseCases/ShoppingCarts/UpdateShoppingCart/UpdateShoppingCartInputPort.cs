using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.ShoppingCarts.UpdateShoppingCart;

public record UpdateShoppingCartInputPort(PropertyChangeTracker<string> Name, long ShoppingCartID) : IInputPort<IUpdateShoppingCartOutputPort>;
