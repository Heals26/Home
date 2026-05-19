using AutoMapper;
using Home.Application.UseCases.ShoppingLists.DeleteShoppingList;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingLists.DeleteShoppingList;

public class DeleteShoppingListPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteShoppingListOutputPort
{

    #region Methods

    Task IDeleteShoppingListOutputPort.PresentShoppingListDeletedNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
