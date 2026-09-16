namespace Home.WebUI.Infrastructure.Services.ShoppingLists;

/// <summary>
/// Whether this device is shopping with a list. Kept on the device, because the aisle layout is each
/// phone's own choice while the shop itself belongs to the list.
/// </summary>
public interface IShoppingModeLogic
{

    #region Methods

    /// <summary>
    /// Whether this device joined the shop going on with the list. A shop that has since ended, or a
    /// later one this device never joined, does not count.
    /// </summary>
    Task<bool> IsShoppingAsync(long shoppingListID, long? openShoppingTripID, CancellationToken cancellationToken);

    Task JoinAsync(long shoppingListID, long shoppingTripID, CancellationToken cancellationToken);

    Task LeaveAsync(long shoppingListID, CancellationToken cancellationToken);

    #endregion Methods

}
