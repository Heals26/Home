using Home.WebUI.DataAccess.ShoppingCategories.Models;

namespace Home.WebUI.DataAccess.ShoppingCategories.GetShoppingCategories;

public class GetShoppingCategoriesWebAppResponse
{

    #region Properties

    /// <summary>
    /// The household's aisles, in the order it walks the shop.
    /// </summary>
    public ICollection<ShoppingCategoryDto> ShoppingCategories { get; set; } = [];

    #endregion Properties

}
