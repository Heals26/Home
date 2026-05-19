namespace Home.WebApi.UseCases.ShoppingListItems.CreateShoppingListItem;

public class CreateShoppingListItemApiRequest
{

    #region Properties

    public decimal? Cost { get; set; }
    public bool InBasket { get; set; }
    public string Name { get; set; }
    public decimal? Quantity { get; set; }
    public long ShoppingListID { get; set; }
    public decimal? Volume { get; set; }
    public decimal? Weight { get; set; }

    #endregion Properties

}
