namespace Home.Application.UseCases.ShoppingCarts.DeleteShoppingCart;

public interface IDeleteShoppingCartOutputPort
{

    #region Properties

    Task PresentShoppingCartDeletedNoContentAsync(CancellationToken cancellationToken);

    #endregion Properties

}
