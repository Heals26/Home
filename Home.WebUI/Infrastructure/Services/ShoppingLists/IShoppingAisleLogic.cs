using Home.WebUI.DataAccess.ShoppingCategories.Models;
using Home.WebUI.DataAccess.ShoppingLists.Models;

namespace Home.WebUI.Infrastructure.Services.ShoppingLists;

/// <summary>
/// How a list reads in the shop: under the household's aisles, and what it will roughly come to.
/// </summary>
public interface IShoppingAisleLogic
{

    #region Methods

    /// <summary>
    /// What the lines come to, taking an unpriced line at what it cost last time.
    /// </summary>
    ShoppingListEstimate Estimate(IEnumerable<ShoppingListItemDto> items);

    /// <summary>
    /// Another aisle already called this, in any case, or null. The aisle being renamed is passed so
    /// it can keep its own name in a different case.
    /// </summary>
    ShoppingCategoryDto? FindNameClash(IEnumerable<ShoppingCategoryDto> aisles, string name, long? exceptShoppingCategoryID);

    /// <summary>
    /// The items under the household's aisles in walking order, then whatever has no aisle yet.
    /// Aisles with nothing on the list are left out.
    /// </summary>
    IReadOnlyList<ShoppingAisleGroup> Group(IEnumerable<ShoppingListItemDto> items, IEnumerable<ShoppingCategoryDto> aisles);

    #endregion Methods

}
