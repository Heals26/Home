using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingCarts.GetShoppingCarts;

internal class GetShoppingCartsInteractor : IInteractor<GetShoppingCartsInputPort, IGetShoppingCartsOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetShoppingCartsInputPort input,
        IGetShoppingCartsOutputPort output,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _ShoppingCarts = _PersistenceContext.GetEntities<ShoppingCart>()
            .Select(sc => new
            {
                ShoppingCart = sc,
                sc.Items,
                sc.CreatedBy
            })
            .ToList()
            .Select(sc => sc.ShoppingCart);

        await output.PresentShoppingCartsAsync(_ShoppingCarts, cancellationToken).ConfigureAwait(false);
    }

    #endregion Methods

}
