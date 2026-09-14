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

    /// <summary>
    /// A memory that does not exist yet is saved straight away, along with anything else the request
    /// already has waiting to save.
    /// </summary>
    Task<ShoppingItemMemory> GetOrCreateAsync(Household household, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Remembers what a line in the trolley cost, or takes back what the same line recorded moments
    /// ago when it comes out of the trolley or loses its price.
    /// </summary>
    Task RecordTickAsync(Household household, ShoppingListItem item, CancellationToken cancellationToken);

    #endregion Methods

}
