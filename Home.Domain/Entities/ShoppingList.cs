namespace Home.Domain.Entities;

public class ShoppingList
{

    #region Properties

    public long ShoppingListID { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Audit> Audits { get; set; } = [];
    public Household Household { get; set; } = null!;
    public ICollection<ShoppingListItem> Items { get; set; } = [];

    #endregion Properties

}
