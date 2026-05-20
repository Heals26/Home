namespace Home.WebUI.DataAccess.ShoppingLists.GetShoppingLists;

public class GetShoppingListsWebAppResponse
{

    #region Properties

    /// <summary>
    /// A collection of shopping lists.
    /// </summary>
    public ICollection<GetShoppingListDto> ShoppingLists { get; set; } = [];

    #endregion Properties

}

public class GetShoppingListDto
{

    #region Properties

    /// <summary>
    /// The number of items in the shopping list.
    /// </summary>
    public long ItemCount { get; set; }

    /// <summary>
    /// The name of the shopping list.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the shopping list.
    /// </summary>
    public long ShoppingListID { get; set; }

    #endregion Properties

}
