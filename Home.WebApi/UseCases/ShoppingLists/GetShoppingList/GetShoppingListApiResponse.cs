using Home.WebApi.UseCases.ShoppingLists.Models;

namespace Home.WebApi.UseCases.ShoppingLists.GetShoppingList;

public class GetShoppingListApiResponse
{

    #region Properties

    public bool GroupByAisle { get; set; }

    /// <summary>
    /// The name of the shopping list
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The items in the shopping list
    /// </summary>
    public List<ShoppingListItemDto> Items { get; set; }

    /// <summary>
    /// The shop going on with the list, or null when nobody is shopping with it. A device that joined
    /// this trip shows the list the way it reads in the aisle.
    /// </summary>
    public long? ShoppingTripID { get; set; }

    #endregion Properties

}
