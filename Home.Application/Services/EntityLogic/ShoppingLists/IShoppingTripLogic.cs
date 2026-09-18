using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.ShoppingLists;

/// <summary>
/// A shop with a list, from when someone switches shopping mode on until Done or two quiet hours.
/// </summary>
public interface IShoppingTripLogic
{

    #region Methods

    /// <summary>
    /// Ends the trip going on with the list. False when nobody was shopping with it.
    /// </summary>
    bool End(Household household, long shoppingListID);

    /// <summary>
    /// The trip going on with the list, or null when nobody is shopping with it.
    /// </summary>
    ShoppingTrip? FindOpen(Household household, long shoppingListID);

    /// <summary>
    /// The trip going on with the list, held off from the quiet spell that would otherwise end it,
    /// because something has just happened on the list. Null when nobody is shopping with it.
    /// </summary>
    ShoppingTrip? RecordActivityOn(Household household, long shoppingListID);

    /// <summary>
    /// A new trip is saved straight away, along with anything else the request already has waiting
    /// to save. When another request starts one on the same list at the same moment, both carry on
    /// with whichever was saved first.
    /// </summary>
    Task<ShoppingTrip> StartAsync(Household household, ShoppingList shoppingList, CancellationToken cancellationToken);

    #endregion Methods

}
