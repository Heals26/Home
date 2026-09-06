using AutoMapper;
using Home.Application.UseCases.ShoppingListItems.MoveShoppingListItem;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingListItems.MoveShoppingListItem;

public class MoveShoppingListItemPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IMoveShoppingListItemOutputPort
{

    #region Methods

    Task IMoveShoppingListItemOutputPort.PresentShoppingListItemMovedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IMoveShoppingListItemOutputPort.PresentShoppingListItemNotFoundAsync(long shoppingListItemID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List Item {shoppingListItemID} Not Found", cancellationToken);

    Task IMoveShoppingListItemOutputPort.PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List {shoppingListID} Not Found", cancellationToken);

    #endregion Methods

}
