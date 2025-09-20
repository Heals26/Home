namespace Home.Domain.Entities;

public class ShoppingCart
{

    #region Properties

    public long ShoppingCartID { get; set; }
    public string Name { get; set; }

    public ICollection<Audit> Audits { get; set; }
    public ICollection<ShoppingCartItem> Items { get; set; }

    #endregion Properties

}
