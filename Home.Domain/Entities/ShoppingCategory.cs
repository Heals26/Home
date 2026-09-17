using Home.Domain.Deletions;

namespace Home.Domain.Entities;

/// <summary>
/// A part of the shop, such as Dairy or Frozen. The household names them and puts them in the order
/// its own shop is laid out, which is the order a list grouped by aisle follows.
/// </summary>
public class ShoppingCategory : ISoftDeletable
{

    #region Properties

    public long ShoppingCategoryID { get; set; }

    public DateTime? DeletedOnUTC { get; set; }

    public Household Household { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public int Sequence { get; set; }

    #endregion Properties

}
