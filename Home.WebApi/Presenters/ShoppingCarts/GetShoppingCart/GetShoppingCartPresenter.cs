using AutoMapper;
using Home.Application.UseCases.ShoppingCarts.GetShoppingCart;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingCarts.GetShoppingCart;

namespace Home.WebApi.Presenters.ShoppingCarts.GetShoppingCart;

public class GetShoppingCartPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetShoppingCartOutputPort
{

    #region Methods

    Task IGetShoppingCartOutputPort.PresentShoppingCartAsync(ShoppingCart shoppingCart, CancellationToken cancellationToken)
        => this.OkAsync(mapper.Map<GetShoppingCartApiResponse>(shoppingCart), cancellationToken);

    #endregion Methods

}
