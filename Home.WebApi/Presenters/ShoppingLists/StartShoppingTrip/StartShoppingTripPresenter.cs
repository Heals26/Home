using AutoMapper;
using Home.Application.UseCases.ShoppingLists.StartShoppingTrip;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingLists.StartShoppingTrip;

namespace Home.WebApi.Presenters.ShoppingLists.StartShoppingTrip;

public class StartShoppingTripPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IStartShoppingTripOutputPort
{

    #region Methods

    Task IStartShoppingTripOutputPort.PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List {shoppingListID} Not Found", cancellationToken);

    Task IStartShoppingTripOutputPort.PresentShoppingTripAsync(ShoppingTrip shoppingTrip, CancellationToken cancellationToken)
        => this.OkAsync(new StartShoppingTripApiResponse() { ShoppingTripID = shoppingTrip.ShoppingTripID }, cancellationToken);

    #endregion Methods

}
