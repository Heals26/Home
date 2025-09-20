using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCarts.UpdateShoppingCart;

public interface IUpdateShoppingCartOutputPort : IAuthenticationFailureOutputPort
{

    #region Methods

    Task PresentShoppingCartNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
