using AutoMapper;
using Home.Application.UseCases.ShoppingLists.GetShoppingList;
using Home.Application.UseCases.ShoppingLists.Models;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingLists.GetShoppingList;

namespace Home.WebApi.Presenters.ShoppingLists.GetShoppingList;

public class GetShoppingListPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetShoppingListOutputPort
{

    #region Methods

    Task IGetShoppingListOutputPort.PresentShoppingListAsync(ShoppingList shoppingList, IReadOnlyDictionary<long, ShoppingItemInsight> insights, ShoppingTrip? openTrip, CancellationToken cancellationToken)
    {
        var _Response = mapper.Map<GetShoppingListApiResponse>(shoppingList);
        _Response.ShoppingTripID = openTrip?.ShoppingTripID;

        foreach (var _Item in _Response.Items)
        {
            if (!insights.TryGetValue(_Item.ShoppingListItemID, out var _Insight))
                continue;

            _Item.EstimatedCost = _Insight.EstimatedCost;
            _Item.IsDearerThanUsual = _Insight.IsDearerThanUsual;
            _Item.ShoppingCategoryID = _Insight.ShoppingCategoryID;
            _Item.UsualCost = _Insight.UsualCost;
        }

        return this.OkAsync(_Response, cancellationToken);
    }

    Task IGetShoppingListOutputPort.PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List {shoppingListID} Not Found", cancellationToken);

    #endregion Methods

}
