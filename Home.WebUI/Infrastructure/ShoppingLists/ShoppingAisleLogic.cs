using Home.WebUI.DataAccess.ShoppingCategories.Models;
using Home.WebUI.DataAccess.ShoppingLists.Models;
using Home.WebUI.Infrastructure.Services.ShoppingLists;

namespace Home.WebUI.Infrastructure.ShoppingLists;

public class ShoppingAisleLogic : IShoppingAisleLogic
{

    #region Methods

    /// <summary>
    /// A cost is what the line costs, not a price per kilo, so it is added as it stands. Multiplying
    /// it by the amount would turn "$3.50 for 2 kg of potatoes" into seven dollars.
    /// </summary>
    ShoppingListEstimate IShoppingAisleLogic.Estimate(IEnumerable<ShoppingListItemDto> items)
    {
        var _Items = items.ToList();

        return new(
            _Items.Any(i => i.Cost == null && i.EstimatedCost != null),
            _Items.Sum(i => i.Cost ?? i.EstimatedCost ?? 0),
            _Items.Count(i => i.Cost == null && i.EstimatedCost == null));
    }

    ShoppingCategoryDto? IShoppingAisleLogic.FindNameClash(IEnumerable<ShoppingCategoryDto> aisles, string name, long? exceptShoppingCategoryID)
        => aisles.FirstOrDefault(a => a.ShoppingCategoryID != exceptShoppingCategoryID
            && string.Equals(a.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));

    IReadOnlyList<ShoppingAisleGroup> IShoppingAisleLogic.Group(IEnumerable<ShoppingListItemDto> items, IEnumerable<ShoppingCategoryDto> aisles)
    {
        var _Aisles = aisles.ToDictionary(a => a.ShoppingCategoryID);

        // An aisle removed on another phone can still be named by a list loaded before it went, so an
        // item filed under one the household no longer has reads as filed under none.
        var _Groups = items
            .OrderBy(i => i.Sequence)
            .ThenBy(i => i.ShoppingListItemID)
            .GroupBy(i => i.ShoppingCategoryID is { } _ID && _Aisles.TryGetValue(_ID, out var _Aisle) ? _Aisle : null)
            .ToList();

        return
        [
            .. _Groups
                .Where(g => g.Key != null)
                .OrderBy(g => g.Key!.Sequence)
                .ThenBy(g => g.Key!.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => new ShoppingAisleGroup(g.Key, [.. g])),
            .. _Groups
                .Where(g => g.Key == null)
                .Select(g => new ShoppingAisleGroup(null, [.. g]))
        ];
    }

    #endregion Methods

}
