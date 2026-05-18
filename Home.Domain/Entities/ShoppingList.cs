namespace Home.Domain.Entities;

public class ShoppingList
{

    #region Properties

    public long ShoppingListID { get; set; }
    public string Name { get; set; }

    public ICollection<Audit> Audits { get; set; }
    public Household Household { get; set; }
    public ICollection<ShoppingListItem> Items { get; set; }

    #endregion Properties

}
