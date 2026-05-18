using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingLists.UpdateShoppingList;

internal class UpdateShoppingListInteractor : IInteractor<UpdateShoppingListInputPort, IUpdateShoppingListOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateShoppingListInputPort inputPort,
        IUpdateShoppingListOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();

        var _ShoppingList = _PersistenceContext.Find<ShoppingList>(inputPort.ShoppingListID);

        if (inputPort.Name.HasBeenSet)
            _ShoppingList.Name = inputPort.Name.Value;

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentShoppingListNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
