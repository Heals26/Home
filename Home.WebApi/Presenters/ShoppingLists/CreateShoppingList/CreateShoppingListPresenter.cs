using AutoMapper;
using Home.Application.UseCases.ShoppingLists.CreateShoppingList;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingLists.CreateShoppingList;

namespace Home.WebApi.Presenters.ShoppingLists.CreateShoppingList;

public class CreateShoppingListPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateShoppingListOutputPort
{

    #region Methods

    Task ICreateShoppingListOutputPort.PresentShoppingListCreatedAsync(long shoppingListID, CancellationToken cancellationToken)
        => this.CreatedAsync(shoppingListID, new CreateShoppingListApiResponse() { ShoppingListID = shoppingListID }, cancellationToken);

    #endregion Methods

}
