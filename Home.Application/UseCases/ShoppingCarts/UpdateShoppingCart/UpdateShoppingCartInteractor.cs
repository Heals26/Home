using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingCarts.UpdateShoppingCart;

internal class UpdateShoppingCartInteractor : IInteractor<UpdateShoppingCartInputPort, IUpdateShoppingCartOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateShoppingCartInputPort inputPort,
        IUpdateShoppingCartOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _ShoppingCart = _PersistenceContext.Find<ShoppingCart>(inputPort.ShoppingCartID);

        if (inputPort.Name.HasBeenSet)
            _ShoppingCart.Name = inputPort.Name.Value;

        _ = _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentShoppingCartNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
