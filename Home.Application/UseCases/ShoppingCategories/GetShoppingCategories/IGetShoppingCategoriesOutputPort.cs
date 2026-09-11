using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingCategories.GetShoppingCategories;

public interface IGetShoppingCategoriesOutputPort
{

    #region Methods

    Task PresentShoppingCategoriesAsync(IEnumerable<ShoppingCategory> shoppingCategories, CancellationToken cancellationToken);

    #endregion Methods

}
