using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.ShoppingCartItems.UpdateShoppingCartItem;

public record UpdateShoppingCartItemInputPort(
    PropertyChangeTracker<int> Cost,
    PropertyChangeTracker<bool> InBasket,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<int> Quantity,
    long ShoppingCartItemID,
    PropertyChangeTracker<long> Sequence)
    : IInputPort<IUpdateShoppingCartItemOutputPort>;
