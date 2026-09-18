using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.ShoppingListItems.UpdateShoppingListItem;

/// <summary>
/// Updates the shopping list item
/// </summary>
public record UpdateShoppingListItemApiRequest(
    PropertyChangeTracker<decimal?> Amount,
    PropertyChangeTracker<decimal?> Cost,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<string?> Note,
    long ShoppingListItemID,
    PropertyChangeTracker<long?> Unit);
