using AutoMapper;
using Home.Application.UseCases.ShoppingCarts.DeleteShoppingCart;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingCarts.DeleteShoppingCart;

public class DeleteShoppingCartPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteShoppingCartOutputPort
{

    #region Methods

    Task IDeleteShoppingCartOutputPort.PresentShoppingCartDeletedNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
