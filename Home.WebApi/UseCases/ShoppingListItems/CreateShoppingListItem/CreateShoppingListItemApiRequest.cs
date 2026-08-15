namespace Home.WebApi.UseCases.ShoppingListItems.CreateShoppingListItem;

public class CreateShoppingListItemApiRequest
{

    #region Properties

    /// <summary>
    /// How much to buy, in <see cref="Unit"/>.
    /// </summary>
    public decimal? Amount { get; set; }

    public decimal? Cost { get; set; }
    public bool InBasket { get; set; }
    public string Name { get; set; }
    public long ShoppingListID { get; set; }
    public long? Unit { get; set; }

    #endregion Properties

}
