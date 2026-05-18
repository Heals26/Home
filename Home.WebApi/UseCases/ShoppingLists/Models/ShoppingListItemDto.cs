namespace Home.WebApi.UseCases.ShoppingLists.Models;

public class ShoppingListItemDto
{

    #region Properties

    public decimal? Cost { get; set; }
    public bool InBasket { get; set; }
    public long ShoppingListItemID { get; set; }
    public string Name { get; set; }
    public decimal? Quantity { get; set; }
    public long Sequence { get; set; }
    public decimal? Volume { get; set; }
    public decimal? Weight { get; set; }

    #endregion Properties

}
