using AutoMapper;
using Home.Application.UseCases.ShoppingLists.GetShoppingLists;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingLists.GetShoppingLists;

namespace Home.WebApi.Presenters.ShoppingLists.GetShoppingLists;

public class GetShoppingListsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetShoppingListsOutputPort
{

    #region Methods

    Task IGetShoppingListsOutputPort.PresentShoppingListsAsync(IEnumerable<ShoppingList> shoppingLists, CancellationToken cancellationToken)
        => this.OkAsync(mapper.Map<GetShoppingListsApiResponse>(shoppingLists), cancellationToken);

    #endregion Methods

}
