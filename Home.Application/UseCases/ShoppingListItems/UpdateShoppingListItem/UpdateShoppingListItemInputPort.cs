using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;

public record UpdateShoppingListItemInputPort(
    PropertyChangeTracker<decimal?> Cost,
    PropertyChangeTracker<bool> InBasket,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<decimal?> Quantity,
    long ShoppingListItemID,
    PropertyChangeTracker<long> Sequence,
    PropertyChangeTracker<decimal?> Volume,
    PropertyChangeTracker<decimal?> Weight)
    : IInputPort<IUpdateShoppingListItemOutputPort>;
