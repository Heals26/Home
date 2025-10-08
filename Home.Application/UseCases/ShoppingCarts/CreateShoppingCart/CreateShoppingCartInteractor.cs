using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingCarts.CreateShoppingCart;

internal class CreateShoppingCartInteractor : IInteractor<CreateShoppingCartInputPort, ICreateShoppingCartOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateShoppingCartInputPort inputPort,
        ICreateShoppingCartOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<ShoppingCart>>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _ShoppingCart = new ShoppingCart()
        {
            CreatedBy = _AuthorisationService.GetUser(),
            Items = [],
            Name = inputPort.Name
        };

        _AuditLogic.AddAudit(_ShoppingCart);

        _PersistenceContext.Add(_ShoppingCart);
        _ = _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentShoppingCartCreatedAsync(_ShoppingCart.ShoppingCartID, cancellationToken);
    }

    #endregion Methods

}
