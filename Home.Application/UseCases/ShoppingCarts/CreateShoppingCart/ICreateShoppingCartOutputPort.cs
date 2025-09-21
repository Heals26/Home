namespace Home.Application.UseCases.ShoppingCarts.CreateShoppingCart;

public interface ICreateShoppingCartOutputPort
{

    #region Methods

    Task PresentShoppingCartCreatedAsync(long shoppingCartID, CancellationToken cancellationToken);

    #endregion Methods

}
