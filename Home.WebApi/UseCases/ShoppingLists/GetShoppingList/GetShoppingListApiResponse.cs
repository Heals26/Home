using Home.WebApi.UseCases.ShoppingLists.Models;

namespace Home.WebApi.UseCases.ShoppingLists.GetShoppingList;

public class GetShoppingListApiResponse
{

    #region Properties

    /// <summary>
    /// The name of the shopping list
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The items in the shopping list
    /// </summary>
    public List<ShoppingListItemDto> Items { get; set; }

    #endregion Properties

}
