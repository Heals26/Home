using AutoMapper;
using Home.Application.UseCases.ShoppingListItems.GetShoppingListItems;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItems;

namespace Home.WebApi.Presenters.ShoppingListItems.GetShoppingListItems;

public class GetShoppingListItemsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetShoppingListItemsOutputPort
{

    #region Methods

    Task IGetShoppingListItemsOutputPort.PresentShoppingListItemsAsync(IQueryable<ShoppingListItem> shoppingListItems, CancellationToken cancellationToken)
        => this.OkAsync(new GetShoppingListItemsApiResponse(
            [.. shoppingListItems.Select(sli => new GetShoppingListItemDto(
                sli.Cost,
                sli.InBasket,
                sli.Name,
                sli.Quantity,
                sli.Sequence,
                sli.ShoppingListItemID,
                sli.Volume,
                sli.Weight))]
        ), cancellationToken);

    Task IGetShoppingListItemsOutputPort.PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List {shoppingListID} Not Found", cancellationToken);

    #endregion Methods

}
