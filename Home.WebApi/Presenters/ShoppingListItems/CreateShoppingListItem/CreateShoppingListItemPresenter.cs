using AutoMapper;
using Home.Application.UseCases.ShoppingListItems.CreateShoppingListItem;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingListItems.CreateShoppingListItem;

namespace Home.WebApi.Presenters.ShoppingListItems.CreateShoppingListItem;

public class CreateShoppingListItemPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateShoppingListItemOutputPort
{

    #region Methods

    Task ICreateShoppingListItemOutputPort.PresentShoppingListItemCreatedAsync(long shoppingListItemID, CancellationToken cancellationToken)
        => this.CreatedAsync(shoppingListItemID, new CreateShoppingListItemApiResponse() { ShoppingListItemID = shoppingListItemID }, cancellationToken);

    Task ICreateShoppingListItemOutputPort.PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List {shoppingListID} Not Found", cancellationToken);

    #endregion Methods

}
