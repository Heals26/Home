using Home.Application.UseCases.ShoppingLists.Models;
using Home.Domain.Entities;

namespace Home.Application.Services.EntityLogic.ShoppingLists;

/// <summary>
/// The household's memory of what it buys: which aisle an item goes in and what it has cost.
/// </summary>
public interface IShoppingItemMemoryLogic
{

    #region Methods

    /// <summary>
    /// What the memory says about each line, keyed by the line's ID.
    /// </summary>
    IReadOnlyDictionary<long, ShoppingItemInsight> Assess(Household household, IEnumerable<ShoppingListItem> items);

    ShoppingItemMemory GetOrCreate(Household household, string name);

    /// <summary>
    /// Remembers what a line in the trolley cost, or takes back what the same line recorded moments
    /// ago when it comes out of the trolley or loses its price.
    /// </summary>
    void RecordTick(Household household, ShoppingListItem item);

    #endregion Methods

}
