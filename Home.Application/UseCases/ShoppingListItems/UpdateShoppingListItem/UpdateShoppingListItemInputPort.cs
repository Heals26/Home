using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;

/// <summary>
/// What the sheet behind a line can change. Ticking it and moving it are their own use cases, so
/// what is left here is a form someone filled in.
/// </summary>
public record UpdateShoppingListItemInputPort(
    PropertyChangeTracker<decimal?> Amount,
    PropertyChangeTracker<decimal?> Cost,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<string?> Note,
    long ShoppingListItemID,
    PropertyChangeTracker<long?> Unit)
    : IInputPort<IUpdateShoppingListItemOutputPort>;
