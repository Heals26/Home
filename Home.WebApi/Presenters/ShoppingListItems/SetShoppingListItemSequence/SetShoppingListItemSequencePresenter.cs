using AutoMapper;
using Home.Application.UseCases.ShoppingListItems.SetShoppingListItemSequence;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingListItems.SetShoppingListItemSequence;

public class SetShoppingListItemSequencePresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ISetShoppingListItemSequenceOutputPort
{

    #region Methods

    Task ISetShoppingListItemSequenceOutputPort.PresentShoppingListItemNotFoundAsync(long shoppingListItemID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping List Item {shoppingListItemID} Not Found", cancellationToken);

    Task ISetShoppingListItemSequenceOutputPort.PresentShoppingListItemSequenceSetAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
