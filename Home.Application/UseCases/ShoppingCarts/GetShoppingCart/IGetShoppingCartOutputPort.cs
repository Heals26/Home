using CleanArchitecture.Mediator;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingCarts.GetShoppingCart;

public interface IGetShoppingCartOutputPort : IAuthenticationFailureOutputPort
{

    #region Methods

    Task PresentShoppingCartAsync(ShoppingCart shoppingCart, CancellationToken cancellationToken);

    #endregion Methods

}
