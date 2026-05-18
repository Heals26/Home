using AutoMapper;
using Home.Application.UseCases.ShoppingListItems.DeleteShoppingListItem;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingListItems.DeleteShoppingListItem;

public class DeleteShoppingListItemPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteShoppingListItemOutputPort
{

    #region Methods

    Task IDeleteShoppingListItemOutputPort.PresentShoppingListItemNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IDeleteShoppingListItemOutputPort.PresentShoppingListItemNotFoundAsync(long shoppingListItemID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List Item {shoppingListItemID} Not Found", cancellationToken);

    #endregion Methods

}
