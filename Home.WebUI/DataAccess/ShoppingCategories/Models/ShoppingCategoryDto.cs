namespace Home.WebUI.DataAccess.ShoppingCategories.Models;

public class ShoppingCategoryDto
{

    #region Properties

    /// <summary>
    /// What the household calls this part of the shop.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Where it comes on the household's walk through the shop.
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// The ID of the aisle.
    /// </summary>
    public long ShoppingCategoryID { get; set; }

    #endregion Properties

}
