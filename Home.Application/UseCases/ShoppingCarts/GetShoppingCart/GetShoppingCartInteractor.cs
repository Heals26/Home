using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingCarts.GetShoppingCart;

internal class GetShoppingCartInteractor : IInteractor<GetShoppingCartInputPort, IGetShoppingCartOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetShoppingCartInputPort inputPort,
        IGetShoppingCartOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _ShoppingCart = _PersistenceContext.GetEntities<ShoppingCart>()
            .Where(sc => sc.ShoppingCartID == inputPort.ShoppingCartID)
            .Select(sc => new
            {
                ShoppingCart = sc,
                sc.Items
            })
            .Single()
            .ShoppingCart;

        await outputPort.PresentShoppingCartAsync(_ShoppingCart, cancellationToken);
    }

    #endregion Methods

}
