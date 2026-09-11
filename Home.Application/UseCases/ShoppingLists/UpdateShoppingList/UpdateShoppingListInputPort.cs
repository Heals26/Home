using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.ShoppingLists.UpdateShoppingList;

public record UpdateShoppingListInputPort(
    PropertyChangeTracker<bool> GroupByAisle,
    PropertyChangeTracker<bool> IsArchived,
    PropertyChangeTracker<string> Name,
    long ShoppingListID)
    : IInputPort<IUpdateShoppingListOutputPort>;
