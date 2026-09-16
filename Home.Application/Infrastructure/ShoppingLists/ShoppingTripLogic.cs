using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.Infrastructure.ShoppingLists;

public class ShoppingTripLogic(IPersistenceContext persistenceContext, TimeProvider timeProvider) : IShoppingTripLogic
{

    #region Fields

    /// <summary>
    /// How long a trip can go without anything happening before it counts as over, for the shop
    /// nobody remembered to finish.
    /// </summary>
    public static readonly TimeSpan QuietSpell = TimeSpan.FromHours(2);

    #endregion Fields

    #region Properties

    private DateTime NowUTC => timeProvider.GetUtcNow().UtcDateTime;

    #endregion Properties

    #region Methods

    bool IShoppingTripLogic.End(Household household, long shoppingListID)
    {
        var _Trips = this.Open(household, shoppingListID).ToList();

        foreach (var _Trip in _Trips)
            _Trip.EndedOnUTC = this.NowUTC;

        return _Trips.Count > 0;
    }

    ShoppingTrip? IShoppingTripLogic.FindOpen(Household household, long shoppingListID)
        => this.Open(household, shoppingListID).FirstOrDefault();

    /// <summary>
    /// Oldest first. Only two phones starting a shop together leave more than one, and the first
    /// saved is the one both keep.
    /// </summary>
    private IQueryable<ShoppingTrip> Open(Household household, long shoppingListID)
    {
        var _Since = this.NowUTC - QuietSpell;

        return persistenceContext.GetEntities<ShoppingTrip>()
            .Where(t => t.ShoppingList.ShoppingListID == shoppingListID
                && t.ShoppingList.Household.HouseholdID == household.HouseholdID
                && t.EndedOnUTC == null
                && t.LastActivityOnUTC >= _Since)
            .OrderBy(t => t.ShoppingTripID);
    }

    void IShoppingTripLogic.RecordActivity(ShoppingTrip shoppingTrip)
        => shoppingTrip.LastActivityOnUTC = this.NowUTC;

    async Task<ShoppingTrip> IShoppingTripLogic.StartAsync(Household household, ShoppingList shoppingList, CancellationToken cancellationToken)
    {
        var _Trip = new ShoppingTrip()
        {
            LastActivityOnUTC = this.NowUTC,
            ShoppingList = shoppingList,
            StartedOnUTC = this.NowUTC
        };

        persistenceContext.Add(_Trip);
        _ = await persistenceContext.SaveChangesAsync(cancellationToken);

        var _First = this.Open(household, shoppingList.ShoppingListID).First();

        if (_First == _Trip)
            return _Trip;

        persistenceContext.Remove(_Trip);
        _First.LastActivityOnUTC = this.NowUTC;

        return _First;
    }

    #endregion Methods

}
