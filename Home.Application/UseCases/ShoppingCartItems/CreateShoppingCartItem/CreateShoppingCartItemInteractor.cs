using CleanArchitecture.Mediator;
using Home.Application.Services.EntityLogic.ShoppingCarts;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;

namespace Home.Application.UseCases.ShoppingCartItems.CreateShoppingCartItem;

internal class CreateShoppingCartItemInteractor : IInteractor<CreateShoppingCartItemInputPort, ICreateShoppingCartItemOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateShoppingCartItemInputPort inputPort,
        ICreateShoppingCartItemOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuditLogic = serviceFactory.GetService<IAuditLogic<ShoppingCart>>();
        var _ShoppingCartLogic = serviceFactory.GetService<IShoppingCartLogic>();

        var _ShoppingCart = _PersistenceContext.GetEntities<ShoppingCart>()
            .Where(sc => sc.ShoppingCartID == inputPort.ShoppingCartID)
            .Select(sc => new
            {
                ShoppingCart = sc,
                sc.Items
            })
            .SingleOrDefault()
            ?.ShoppingCart;

        if (_ShoppingCart == null)
        {
            await outputPort.PresentShoppingCartNotFoundAsync(inputPort.ShoppingCartID, cancellationToken);
            return;
        }

        var _ShoppingCartItem = _ShoppingCartLogic.AddItem(inputPort);
        _ShoppingCart.Items.Add(_ShoppingCartItem);

        _AuditLogic.UpdateAudit(_ShoppingCart);

        await outputPort.PresentShoppingCartItemCreatedAsync(_ShoppingCartItem.ShoppingCartItemID, cancellationToken);
    }

    #endregion Methods

}
