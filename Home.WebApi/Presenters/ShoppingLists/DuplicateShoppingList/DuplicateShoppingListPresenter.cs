using AutoMapper;
using Home.Application.UseCases.ShoppingLists.DuplicateShoppingList;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingLists.DuplicateShoppingList;

namespace Home.WebApi.Presenters.ShoppingLists.DuplicateShoppingList;

public class DuplicateShoppingListPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDuplicateShoppingListOutputPort
{

    #region Methods

    Task IDuplicateShoppingListOutputPort.PresentShoppingListDuplicatedAsync(long shoppingListID, CancellationToken cancellationToken)
        => this.CreatedAsync(shoppingListID, new DuplicateShoppingListApiResponse() { ShoppingListID = shoppingListID }, cancellationToken);

    Task IDuplicateShoppingListOutputPort.PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List {shoppingListID} Not Found", cancellationToken);

    #endregion Methods

}
