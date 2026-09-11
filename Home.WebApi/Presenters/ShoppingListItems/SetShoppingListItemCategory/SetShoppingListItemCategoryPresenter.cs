using AutoMapper;
using Home.Application.UseCases.ShoppingListItems.SetShoppingListItemCategory;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingListItems.SetShoppingListItemCategory;

public class SetShoppingListItemCategoryPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISetShoppingListItemCategoryOutputPort
{

    #region Methods

    Task ISetShoppingListItemCategoryOutputPort.PresentShoppingCategoryNotFoundAsync(long shoppingCategoryID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping Category {shoppingCategoryID} Not Found", cancellationToken);

    Task ISetShoppingListItemCategoryOutputPort.PresentShoppingListItemCategorySetAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task ISetShoppingListItemCategoryOutputPort.PresentShoppingListItemNotFoundAsync(long shoppingListItemID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List Item {shoppingListItemID} Not Found", cancellationToken);

    #endregion Methods

}
