using Home.WebApi.UseCases.ShoppingCategories.Models;

namespace Home.WebApi.UseCases.ShoppingCategories.GetShoppingCategories;

public class GetShoppingCategoriesApiResponse
{

    #region Properties

    public ICollection<ShoppingCategoryDto> ShoppingCategories { get; set; } = [];

    #endregion Properties

}
