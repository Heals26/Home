using AutoMapper;
using Home.Application.UseCases.ShoppingCarts.UpdateShoppingCart;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingCarts.UpdateShoppingCart;

public class UpdateShoppingCartPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateShoppingCartOutputPort
{

    #region Methods

    Task IUpdateShoppingCartOutputPort.PresentShoppingCartNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
