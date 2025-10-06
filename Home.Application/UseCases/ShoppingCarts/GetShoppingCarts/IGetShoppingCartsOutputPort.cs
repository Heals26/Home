using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingCarts.GetShoppingCarts;

public interface IGetShoppingCartsOutputPort
{

    #region Methods

    Task PresentShoppingCartsAsync(IEnumerable<ShoppingCart> shoppingCarts, CancellationToken cancellationToken);

    #endregion Methods

}
