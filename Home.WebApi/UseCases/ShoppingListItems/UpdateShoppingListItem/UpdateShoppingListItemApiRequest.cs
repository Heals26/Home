using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.ShoppingListItems.UpdateShoppingListItem;

/// <summary>
/// Updates the shopping list item
/// </summary>
public record UpdateShoppingListItemApiRequest(
    PropertyChangeTracker<decimal?> Amount,
    PropertyChangeTracker<decimal?> Cost,
    PropertyChangeTracker<bool> InBasket,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<long> Sequence,
    long ShoppingListItemID,
    PropertyChangeTracker<long?> Unit);
