using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingCartItems.GetShoppingCartItems;

public interface IGetShoppingCartItemsOutputPort
{

    #region Methods

    Task PresentShoppingCartNotFoundAsync(long shoppingCartID, CancellationToken cancellationToken);
    Task PresentShoppingCartItemsAsync(IQueryable<ShoppingCartItem> shoppingCartItems, CancellationToken cancellationToken);

    #endregion Methods

}
