using AutoMapper;
using Home.Application.UseCases.ShoppingCartItems.GetShoppingCartItems;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingCartItems.GetShoppingCartItems;

namespace Home.WebApi.Presenters.ShoppingCartItems.GetShoppingCartItems;

public class GetShoppingCartItemsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetShoppingCartItemsOutputPort
{

    #region Methods

    Task IGetShoppingCartItemsOutputPort.PresentShoppingCartItemsAsync(IQueryable<ShoppingCartItem> shoppingCartItems, CancellationToken cancellationToken)
        => this.OkAsync(new GetShoppingCartItemsApiResponse(
            [.. shoppingCartItems.Select(sci => new GetShoppingCartItemDto(
                sci.Cost,
                sci.InBasket,
                sci.Name,
                sci.Quantity,
                sci.Sequence))]
        ), cancellationToken);

    Task IGetShoppingCartItemsOutputPort.PresentShoppingCartNotFoundAsync(long shoppingCartID, CancellationToken cancellationToken) => throw new NotImplementedException();

    #endregion Methods

}
