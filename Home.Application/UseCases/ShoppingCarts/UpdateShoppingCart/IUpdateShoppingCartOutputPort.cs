namespace Home.Application.UseCases.ShoppingCarts.UpdateShoppingCart;

public interface IUpdateShoppingCartOutputPort
{

    #region Methods

    Task PresentShoppingCartNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
