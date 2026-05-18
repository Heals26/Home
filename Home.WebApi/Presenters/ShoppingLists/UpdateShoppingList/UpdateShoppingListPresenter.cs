using AutoMapper;
using Home.Application.UseCases.ShoppingLists.UpdateShoppingList;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingLists.UpdateShoppingList;

public class UpdateShoppingListPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateShoppingListOutputPort
{

    #region Methods

    Task IUpdateShoppingListOutputPort.PresentShoppingListNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
