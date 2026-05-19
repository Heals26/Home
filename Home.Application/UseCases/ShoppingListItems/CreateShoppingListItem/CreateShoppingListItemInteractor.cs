using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingListItems.CreateShoppingListItem;

internal class CreateShoppingListItemInteractor : IInteractor<CreateShoppingListItemInputPort, ICreateShoppingListItemOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateShoppingListItemInputPort inputPort,
        ICreateShoppingListItemOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<ShoppingList>>();
        var _ShoppingListLogic = serviceFactory.GetService<IShoppingListLogic>();

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
        {
            await outputPort.PresentShoppingListNotFoundAsync(inputPort.ShoppingListID, cancellationToken);
            return;
        }

        var _ShoppingListItem = _ShoppingListLogic.AddItem(inputPort);
        _ShoppingList.Items.Add(_ShoppingListItem);

        _AuditLogic.UpdateAudit(_ShoppingList);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentShoppingListItemCreatedAsync(_ShoppingListItem.ShoppingListItemID, cancellationToken);
    }

    #endregion Methods

}
