using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Application.UseCases.ShoppingLists.Models;
using Home.Domain.Entities;

namespace Home.Application.Infrastructure.ShoppingLists;

public class ShoppingItemMemoryLogic(IPersistenceContext persistenceContext, TimeProvider timeProvider) : IShoppingItemMemoryLogic
{

    #region Fields

    /// <summary>
    /// How long a tick stays correctable: one trip round the shop, but not so long that reusing the
    /// list next week edits last week's purchase instead of recording a new one.
    /// </summary>
    private static readonly TimeSpan s_CorrectionWindow = TimeSpan.FromHours(12);

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

    private ShoppingItemMemory Create(Household household, string name)
    {
        var _Memory = new ShoppingItemMemory()
        {
            Household = household,
            Name = Truncate(name.Trim()),
            NameKey = KeyOf(name),
            Prices = []
        };

        persistenceContext.Add(_Memory);

        return _Memory;
    }

    private ShoppingItemMemory? Find(Household household, string name)
    {
        var _Key = KeyOf(name);

        return persistenceContext.GetEntities<ShoppingItemMemory>()
            .Where(m => m.Household.HouseholdID == household.HouseholdID && m.NameKey == _Key)
            .Select(m => new
            {
                Memory = m,
                m.Prices,
                m.ShoppingCategory
            })
            .SingleOrDefault()
            ?.Memory;
    }

    ShoppingItemMemory IShoppingItemMemoryLogic.GetOrCreate(Household household, string name)
        => this.Find(household, name) ?? this.Create(household, name);

    private static string KeyOf(string name)
        => Truncate(name.Trim().ToLowerInvariant());

    void IShoppingItemMemoryLogic.RecordTick(Household household, ShoppingListItem item)
    {
        decimal? _Paid = item.InBasket && item.Cost is > 0 ? item.Cost : null;

        // Nothing to take back for an item the household has never paid for, so an untick never
        // creates a memory.
        var _Memory = this.Find(household, item.Name)
            ?? (_Paid == null ? null : this.Create(household, item.Name));

        if (_Memory == null)
            return;

        var _Since = this.NowUTC - s_CorrectionWindow;
        var _Recent = _Memory.Prices
            .FirstOrDefault(p => p.ShoppingListItemID == item.ShoppingListItemID && p.BoughtOnUTC >= _Since);

        if (_Paid is { } _Cost)
        {
            if (_Recent == null)
            {
                _Recent = new ShoppingItemPrice()
                {
                    Memory = _Memory,
                    ShoppingListItemID = item.ShoppingListItemID
                };

                _Memory.Prices.Add(_Recent);
                persistenceContext.Add(_Recent);
            }

            _Recent.Amount = item.Amount;
            _Recent.BoughtOnUTC = this.NowUTC;
            _Recent.Cost = _Cost;
            _Recent.Unit = item.Unit;
        }
        else if (_Recent != null)
        {
            _ = _Memory.Prices.Remove(_Recent);
            persistenceContext.Remove(_Recent);
        }
    }

    private static string Truncate(string value)
        => value.Length <= MaximumNameLength ? value : value[..MaximumNameLength];

    #endregion Methods

}
