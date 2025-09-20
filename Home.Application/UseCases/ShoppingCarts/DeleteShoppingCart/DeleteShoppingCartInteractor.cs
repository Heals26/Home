using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingCarts.DeleteShoppingCart;

internal class DeleteShoppingCartInteractor : IInteractor<DeleteShoppingCartInputPort, IDeleteShoppingCartOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteShoppingCartInputPort input,
        IDeleteShoppingCartOutputPort output,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<ShoppingCart>>();

        _PersistenceContext.GetEntities<ShoppingCart>()
            .Where(sc => sc.ShoppingCartID == input.ShoppingCartID)
            .ToList()
            .ForEach(sc =>
            {
                _PersistenceContext.Remove(sc);
                _AuditLogic.DeleteAudit(sc);
            });

        _ = _PersistenceContext.SaveChangesAsync(cancellationToken);

        await output.PresentShoppingCartDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
