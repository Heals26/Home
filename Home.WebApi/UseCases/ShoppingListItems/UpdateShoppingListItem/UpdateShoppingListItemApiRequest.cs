using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.ShoppingListItems.UpdateShoppingListItem;

/// <summary>
/// Updates the shopping list item
/// </summary>
public record UpdateShoppingListItemApiRequest(
    PropertyChangeTracker<decimal?> Cost,
    PropertyChangeTracker<bool> InBasket,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<decimal?> Quantity,
    long ShoppingListItemID,
    PropertyChangeTracker<long> Sequence,
    PropertyChangeTracker<decimal?> Volume,
    PropertyChangeTracker<decimal?> Weight);
