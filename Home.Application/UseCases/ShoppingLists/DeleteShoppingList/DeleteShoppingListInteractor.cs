using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingLists.DeleteShoppingList;

internal class DeleteShoppingListInteractor : IInteractor<DeleteShoppingListInputPort, IDeleteShoppingListOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteShoppingListInputPort input,
        IDeleteShoppingListOutputPort output,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<ShoppingList>>();

        _PersistenceContext.GetEntities<ShoppingList>()
            .Where(sl => sl.ShoppingListID == input.ShoppingListID)
            .ToList()
            .ForEach(sl =>
            {
                _PersistenceContext.Remove(sl);
                _AuditLogic.DeleteAudit(sl);
            });

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await output.PresentShoppingListDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
