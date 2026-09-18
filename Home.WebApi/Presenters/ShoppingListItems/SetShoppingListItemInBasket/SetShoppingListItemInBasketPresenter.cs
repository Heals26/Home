using AutoMapper;
using Home.Application.UseCases.ShoppingListItems.SetShoppingListItemInBasket;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingListItems.SetShoppingListItemInBasket;

public class SetShoppingListItemInBasketPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISetShoppingListItemInBasketOutputPort
{

    #region Methods

    Task ISetShoppingListItemInBasketOutputPort.PresentShoppingListItemInBasketSetAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task ISetShoppingListItemInBasketOutputPort.PresentShoppingListItemNotFoundAsync(long shoppingListItemID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List Item {shoppingListItemID} Not Found", cancellationToken);

    #endregion Methods

}
