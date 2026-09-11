using Home.WebUI.DataAccess.ShoppingLists.Models;

namespace Home.WebUI.DataAccess.ShoppingLists.GetShoppingList;

public class GetShoppingListWebAppResponse
{

    #region Properties

    /// <summary>
    /// Whether the list reads by aisle rather than in the household's own order. Kept on the list,
    /// so every phone shopping from it sees the same thing.
    /// </summary>
    public bool GroupByAisle { get; set; }

    /// <summary>
    /// The items in the shopping list.
    /// </summary>
    public List<ShoppingListItemDto> Items { get; set; } = [];

    /// <summary>
    /// The name of the shopping list.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    #endregion Properties

}
