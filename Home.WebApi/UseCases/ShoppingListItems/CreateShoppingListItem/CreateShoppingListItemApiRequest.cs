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
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// What the name does not say: a brand, a size, which aisle, who it is for.
    /// </summary>
    public string? Note { get; set; }

    public long ShoppingListID { get; set; }
    public long? Unit { get; set; }

    #endregion Properties

}
