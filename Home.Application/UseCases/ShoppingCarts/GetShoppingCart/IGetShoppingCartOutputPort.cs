using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingCarts.GetShoppingCart;

public interface IGetShoppingCartOutputPort
{

    #region Methods

    Task PresentShoppingCartAsync(ShoppingCart shoppingCart, CancellationToken cancellationToken);
    Task PresentShoppingCartNotFoundAsync(long shippingCartID, CancellationToken cancellationToken);

    #endregion Methods

}
