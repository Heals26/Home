using AutoMapper;
using Home.Application.UseCases.ShoppingLists.EndShoppingTrip;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingLists.EndShoppingTrip;

public class EndShoppingTripPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IEndShoppingTripOutputPort
{

    #region Methods

    Task IEndShoppingTripOutputPort.PresentShoppingTripEndedNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
