using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCarts.CreateShoppingCart;

public interface ICreateShoppingCartOutputPort : IAuthenticationFailureOutputPort
{

    #region Methods

    Task PresentShoppingCartCreatedAsync(long shoppingCartID, CancellationToken cancellationToken);

    #endregion Methods

}
