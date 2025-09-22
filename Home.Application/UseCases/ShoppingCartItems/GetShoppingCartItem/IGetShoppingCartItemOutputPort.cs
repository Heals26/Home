using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingCartItems.GetShoppingCartItem;

public interface IGetShoppingCartItemOutputPort
{

    #region Methods

    Task PresentShoppingCartItemAsync(ShoppingCartItem shoppingCartItem, CancellationToken cancellationToken);
    Task PresentShoppingCartItemNotFoundAsync(long shoppingCartItemID, CancellationToken cancellationToken);

    #endregion Methods

}
