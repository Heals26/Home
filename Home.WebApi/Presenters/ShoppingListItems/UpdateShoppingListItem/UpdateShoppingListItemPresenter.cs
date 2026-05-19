using AutoMapper;
using Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingListItems.UpdateShoppingListItem;

public class UpdateShoppingListItemPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateShoppingListItemOutputPort
{

    #region Methods

    Task IUpdateShoppingListItemOutputPort.PresentShoppingListItemNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUpdateShoppingListItemOutputPort.PresentShoppingListItemNotFoundAsync(long shoppingListItemID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List Item {shoppingListItemID} Not Found", cancellationToken);

    #endregion Methods

}
