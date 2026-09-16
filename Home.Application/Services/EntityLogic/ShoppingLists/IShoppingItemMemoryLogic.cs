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
    /// Remembers what a line ticked during a shop cost, keeps that purchase in step with the line
    /// while it stays ticked, and takes it back when the line comes out of the trolley before the
    /// shop is over.
    /// </summary>
    Task RecordTickAsync(Household household, ShoppingListItem item, bool wasInBasket, ShoppingTrip? openTrip, CancellationToken cancellationToken);

    #endregion Methods

}
