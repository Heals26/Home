using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingLists.GetShoppingList;

internal class GetShoppingListInteractor : IInteractor<GetShoppingListInputPort, IGetShoppingListOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetShoppingListInputPort inputPort,
        IGetShoppingListOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _ShoppingList = _PersistenceContext.GetEntities<ShoppingList>()
            .Where(sl => sl.ShoppingListID == inputPort.ShoppingListID)
            .Select(sl => new
            {
                ShoppingList = sl,
                sl.Items
            })
            .SingleOrDefault()
            ?.ShoppingList;

        if (_ShoppingList == null)
            await outputPort.PresentShoppingListNotFoundAsync(inputPort.ShoppingListID, cancellationToken);
        else
            await outputPort.PresentShoppingListAsync(_ShoppingList, cancellationToken);
    }

    #endregion Methods

}
