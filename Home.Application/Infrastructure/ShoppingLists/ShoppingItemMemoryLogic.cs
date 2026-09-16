using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Application.UseCases.ShoppingLists.Models;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Infrastructure.ShoppingLists;

public class ShoppingItemMemoryLogic(IPersistenceContext persistenceContext, TimeProvider timeProvider) : IShoppingItemMemoryLogic
{

    #region Fields

    private const int MaximumNameLength = 200;

    #endregion Fields

    #region Properties

    private DateTime NowUTC => timeProvider.GetUtcNow().UtcDateTime;

    #endregion Properties

    #region Methods

    IReadOnlyDictionary<long, ShoppingItemInsight> IShoppingItemMemoryLogic.Assess(Household household, IEnumerable<ShoppingListItem> items)
    {
        var _Items = items.ToList();
        var _Keys = _Items.Select(i => KeyOf(i.Name)).Distinct().ToList();

        var _Memories = persistenceContext.GetEntities<ShoppingItemMemory>()
            .Where(m => m.Household.HouseholdID == household.HouseholdID && _Keys.Contains(m.NameKey))
            .Select(m => new
            {
                Memory = m,
                m.Prices,
                m.ShoppingCategory
            })
            .ToList()
            .ToDictionary(m => m.Memory.NameKey, m => m.Memory);

        return _Items.ToDictionary(
            i => i.ShoppingListItemID,
            i => ShoppingPriceLogic.Assess(i, _Memories.GetValueOrDefault(KeyOf(i.Name))));
    }

    /// <summary>
    /// When a price typed in after the shop was bought: as late in the trip as anything is known to
    /// have happened, so filling in last week's receipt does not rank it ahead of this week's shop.
    /// </summary>
    private DateTime BoughtOn(Household household, long shoppingTripID, ShoppingTrip? openTrip)
        => shoppingTripID == openTrip?.ShoppingTripID
            ? this.NowUTC
            : persistenceContext.GetEntities<ShoppingTrip>()
                .Where(t => t.ShoppingTripID == shoppingTripID
                    && t.ShoppingList.Household.HouseholdID == household.HouseholdID)
                .Select(t => (DateTime?)t.LastActivityOnUTC)
                .SingleOrDefault() ?? this.NowUTC;

    /// <summary>
    /// Saved before anything is attached to it. Two requests can reach a new item's first purchase or
    /// first aisle together and the unique index lets only one of them add it, so the one that loses
    /// carries on with the memory the other saved instead of failing.
    /// </summary>
    private async Task<ShoppingItemMemory> CreateAsync(Household household, string name, CancellationToken cancellationToken)
    {
        var _Memory = new ShoppingItemMemory()
        {
            Household = household,
            Name = Truncate(name.Trim()),
            NameKey = KeyOf(name)
        };

        persistenceContext.Add(_Memory);

        try
        {
            _ = await persistenceContext.SaveChangesAsync(cancellationToken);

            return _Memory;
        }
        catch (DbUpdateException)
        {
            var _Saved = this.Find(household, name);

            if (_Saved == null)
                throw;

            // Let go of the copy that lost, including from the household's collection, where the next
            // save would otherwise find it and try to add it again.
            persistenceContext.Entity(_Memory).State = EntityState.Detached;
            _ = household.ShoppingItemMemories.Remove(_Memory);

            return _Saved;
        }
    }

    private ShoppingItemMemory? Find(Household household, string name)
    {
        var _Key = KeyOf(name);

        return persistenceContext.GetEntities<ShoppingItemMemory>()
            .Where(m => m.Household.HouseholdID == household.HouseholdID && m.NameKey == _Key)
            .Select(m => new
            {
                Memory = m,
                m.ShoppingCategory
            })
            .SingleOrDefault()
            ?.Memory;
    }

    Task<ShoppingItemMemory> IShoppingItemMemoryLogic.GetOrCreateAsync(Household household, string name, CancellationToken cancellationToken)
        => this.GetOrCreateAsync(household, name, cancellationToken);

    private async Task<ShoppingItemMemory> GetOrCreateAsync(Household household, string name, CancellationToken cancellationToken)
        => this.Find(household, name) ?? await this.CreateAsync(household, name, cancellationToken);

    private static string KeyOf(string name)
        => Truncate(name.Trim().ToLowerInvariant());

    async Task IShoppingItemMemoryLogic.RecordTickAsync(Household household, ShoppingListItem item, bool wasInBasket, ShoppingTrip? openTrip, CancellationToken cancellationToken)
    {
        if (item.InBasket && !wasInBasket)
            item.ShoppingTripID = openTrip?.ShoppingTripID;

        if (item.ShoppingTripID is not { } _TripID)
            return;

        // Found by the line and its trip rather than by the item's name, so renaming a line after it
        // was ticked moves the purchase it recorded instead of recording another under the new name.
        var _Purchases = persistenceContext.GetEntities<ShoppingItemPrice>()
            .Where(p => p.ShoppingListItemID == item.ShoppingListItemID
                && p.ShoppingTripID == _TripID
                && p.Memory.Household.HouseholdID == household.HouseholdID)
            .ToList();

        if (!item.InBasket)
        {
            if (_TripID == openTrip?.ShoppingTripID)
                persistenceContext.RemoveRange(_Purchases);

            item.ShoppingTripID = null;
            return;
        }

        if (item.Cost is not { } _Cost || _Cost <= 0)
        {
            persistenceContext.RemoveRange(_Purchases);
            return;
        }

        var _Memory = await this.GetOrCreateAsync(household, item.Name, cancellationToken);
        var _Price = _Purchases.FirstOrDefault();

        if (_Price == null)
        {
            _Price = new ShoppingItemPrice()
            {
                BoughtOnUTC = this.BoughtOn(household, _TripID, openTrip),
                ShoppingListItemID = item.ShoppingListItemID,
                ShoppingTripID = _TripID
            };

            persistenceContext.Add(_Price);
        }

        _Price.Amount = item.Amount;
        _Price.Cost = _Cost;
        _Price.Memory = _Memory;
        _Price.Unit = item.Unit;
    }

    private static string Truncate(string value)
        => value.Length <= MaximumNameLength ? value : value[..MaximumNameLength];

    #endregion Methods

}
