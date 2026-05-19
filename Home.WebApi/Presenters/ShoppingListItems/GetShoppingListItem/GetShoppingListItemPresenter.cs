using AutoMapper;
using Home.Application.UseCases.ShoppingListItems.GetShoppingListItem;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItem;

namespace Home.WebApi.Presenters.ShoppingListItems.GetShoppingListItem;

public class GetShoppingListItemPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetShoppingListItemOutputPort
{

    #region Methods

    Task IGetShoppingListItemOutputPort.PresentShoppingListItemAsync(ShoppingListItem shoppingListItem, CancellationToken cancellationToken)
        => this.OkAsync(new GetShoppingListItemApiResponse(
            shoppingListItem.Cost,
            shoppingListItem.InBasket,
            shoppingListItem.Name,
            shoppingListItem.Quantity,
            shoppingListItem.Sequence,
            shoppingListItem.ShoppingListItemID,
            shoppingListItem.Volume,
            shoppingListItem.Weight),
            cancellationToken);

    Task IGetShoppingListItemOutputPort.PresentShoppingListItemNotFoundAsync(long shoppingListItemID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List Item {shoppingListItemID} Not Found", cancellationToken);

    #endregion Methods

}
