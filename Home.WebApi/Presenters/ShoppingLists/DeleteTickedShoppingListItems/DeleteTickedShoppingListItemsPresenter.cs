using AutoMapper;
using Home.Application.UseCases.ShoppingLists.DeleteTickedShoppingListItems;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingLists.DeleteTickedShoppingListItems;

public class DeleteTickedShoppingListItemsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteTickedShoppingListItemsOutputPort
{

    #region Methods

    Task IDeleteTickedShoppingListItemsOutputPort.PresentTickedShoppingListItemsDeletedNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
