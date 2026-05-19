using AutoMapper;
using Home.Application.UseCases.ShoppingLists.GetShoppingList;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingLists.GetShoppingList;

namespace Home.WebApi.Presenters.ShoppingLists.GetShoppingList;

public class GetShoppingListPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetShoppingListOutputPort
{

    #region Methods

    Task IGetShoppingListOutputPort.PresentShoppingListAsync(ShoppingList shoppingList, CancellationToken cancellationToken)
        => this.OkAsync(mapper.Map<GetShoppingListApiResponse>(shoppingList), cancellationToken);

    Task IGetShoppingListOutputPort.PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List {shoppingListID} Not Found", cancellationToken);

    #endregion Methods

}
