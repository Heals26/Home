using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingListItems.DeleteShoppingListItem;

internal class DeleteShoppingListItemInteractor : IInteractor<DeleteShoppingListItemInputPort, IDeleteShoppingListItemOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteShoppingListItemInputPort inputPort,
        IDeleteShoppingListItemOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _ShoppingListItem = _PersistenceContext.Find<ShoppingListItem>(inputPort.ShoppingListItemID);

        if (_ShoppingListItem == null)
        {
            await outputPort.PresentShoppingListItemNotFoundAsync(inputPort.ShoppingListItemID, cancellationToken);
            return;
        }

        _PersistenceContext.Remove(_ShoppingListItem);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentShoppingListItemNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
