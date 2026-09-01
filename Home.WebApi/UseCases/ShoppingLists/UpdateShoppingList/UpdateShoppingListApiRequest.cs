using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.ShoppingLists.UpdateShoppingList;

public record UpdateShoppingListApiRequest(
    PropertyChangeTracker<bool> IsArchived,
    PropertyChangeTracker<string> Name);
