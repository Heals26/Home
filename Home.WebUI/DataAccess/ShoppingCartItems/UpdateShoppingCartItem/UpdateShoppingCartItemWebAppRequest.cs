using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.ShoppingCartItems.UpdateShoppingCartItem;

/// <summary>
/// Updates the shopping cart item
/// </summary>
/// <param name="Cost"></param>
/// <param name="InBasket"></param>
/// <param name="Name"></param>
/// <param name="Quantity"></param>
/// <param name="ShoppingCartItemID"></param>
/// <param name="Sequence"></param>
public record UpdateShoppingCartItemWebAppRequest(
    PropertyChangeTracker<int> Cost,
    PropertyChangeTracker<bool> InBasket,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<int> Quantity,
    long ShoppingCartItemID,
    PropertyChangeTracker<long> Sequence);
