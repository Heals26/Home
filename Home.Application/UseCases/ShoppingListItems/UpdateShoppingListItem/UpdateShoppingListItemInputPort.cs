using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;

public record UpdateShoppingListItemInputPort(
    PropertyChangeTracker<decimal?> Amount,
    PropertyChangeTracker<decimal?> Cost,
    PropertyChangeTracker<bool> InBasket,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<long> Sequence,
    long ShoppingListItemID,
    PropertyChangeTracker<long?> Unit)
    : IInputPort<IUpdateShoppingListItemOutputPort>;
