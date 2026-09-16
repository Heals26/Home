using Home.WebUI.Infrastructure.Services.Preferences;
using Home.WebUI.Infrastructure.Services.ShoppingLists;

namespace Home.WebUI.Infrastructure.ShoppingLists;

/// <summary>
/// Remembers the shop this device joined, one per list, so shopping with two lists in the same
/// supermarket does not take either out of the aisle layout.
/// </summary>
public class ShoppingModeLogic(IDevicePreferences devicePreferences) : IShoppingModeLogic
{

    #region Methods

    async Task<bool> IShoppingModeLogic.IsShoppingAsync(long shoppingListID, long? openShoppingTripID, CancellationToken cancellationToken)
        => openShoppingTripID is { } _TripID
            && await devicePreferences.GetAsync(KeyFor(shoppingListID), cancellationToken) == _TripID.ToString();

    Task IShoppingModeLogic.JoinAsync(long shoppingListID, long shoppingTripID, CancellationToken cancellationToken)
        => devicePreferences.SetAsync(KeyFor(shoppingListID), shoppingTripID.ToString(), cancellationToken);

    private static string KeyFor(long shoppingListID)
        => $"shopping-trip-{shoppingListID}";

    Task IShoppingModeLogic.LeaveAsync(long shoppingListID, CancellationToken cancellationToken)
        => devicePreferences.SetAsync(KeyFor(shoppingListID), string.Empty, cancellationToken);

    #endregion Methods

}
