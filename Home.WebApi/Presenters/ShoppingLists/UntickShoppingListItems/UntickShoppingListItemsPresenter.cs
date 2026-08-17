using AutoMapper;
using Home.Application.UseCases.ShoppingLists.UntickShoppingListItems;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingLists.UntickShoppingListItems;

public class UntickShoppingListItemsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUntickShoppingListItemsOutputPort
{

    #region Methods

    Task IUntickShoppingListItemsOutputPort.PresentShoppingListItemsUntickedNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
