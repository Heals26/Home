using Home.WebUI.DataAccess.ShoppingLists.Models;

namespace Home.WebUI.DataAccess.ShoppingLists.GetShoppingList;

public class GetShoppingListWebAppResponse
{

    #region Properties

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
