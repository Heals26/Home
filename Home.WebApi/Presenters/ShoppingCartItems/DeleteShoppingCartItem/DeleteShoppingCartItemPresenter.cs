using AutoMapper;
using Home.Application.UseCases.ShoppingCartItems.DeleteShoppingCartItem;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingCartItems.DeleteShoppingCartItem;

public class DeleteShoppingCartItemPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteShoppingCartItemOutputPort
{

    #region Methods

    Task IDeleteShoppingCartItemOutputPort.PresentShoppingCartItemNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IDeleteShoppingCartItemOutputPort.PresentShoppingCartItemNotFoundAsync(long shoppingCartItemID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping Cart Item Not Found", cancellationToken);

    #endregion Methods

}
