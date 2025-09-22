using AutoMapper;
using Home.Application.UseCases.ShoppingCartItems.UpdateShoppingCartItem;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingCartItems.UpdateShoppingCartItem;

public class UpdateShoppingCartItemPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateShoppingCartItemOutputPort
{

    #region Methods

    Task IUpdateShoppingCartItemOutputPort.PresentShoppingCartItemNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUpdateShoppingCartItemOutputPort.PresentShoppingCartItemNotFoundAsync(long shoppingCartItemID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping Cart Not Found", cancellationToken);

    #endregion Methods

}
