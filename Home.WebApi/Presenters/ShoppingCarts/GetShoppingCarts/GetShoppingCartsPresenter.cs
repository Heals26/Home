using AutoMapper;
using Home.Application.UseCases.ShoppingCarts.GetShoppingCarts;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingCarts.GetShoppingCarts;

namespace Home.WebApi.Presenters.ShoppingCarts.GetShoppingCarts;

public class GetShoppingCartsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetShoppingCartsOutputPort
{

    #region Methods

    Task IGetShoppingCartsOutputPort.PresentShoppingCartsAsync(IEnumerable<ShoppingCart> shoppingCarts, CancellationToken cancellationToken)
        => this.OkAsync(mapper.Map<GetShoppingCartsApiResponse>(shoppingCarts), cancellationToken);

    #endregion Methods

}
