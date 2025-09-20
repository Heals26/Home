using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingCarts.DeleteShoppingCart;

public interface IDeleteShoppingCartOutputPort : IAuthenticationFailureOutputPort
{

    #region Properties

    Task PresentShoppingCartDeletedNoContentAsync(CancellationToken cancellationToken);

    #endregion Properties

}
