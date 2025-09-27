using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingCartItems.DeleteShoppingCartItem;

internal class DeleteShoppingCartItemInteractor : IInteractor<DeleteShoppingCartItemInputPort, IDeleteShoppingCartItemOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteShoppingCartItemInputPort inputPort,
        IDeleteShoppingCartItemOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _ShoppingCartItem = _PersistenceContext.Find<ShoppingCartItem>(inputPort.ShoppingCartItemID);

        if (_ShoppingCartItem == null)
        {
            await outputPort.PresentShoppingCartItemNotFoundAsync(inputPort.ShoppingCartItemID, cancellationToken);
            return;
        }

        _PersistenceContext.Remove(_ShoppingCartItem);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentShoppingCartItemNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
