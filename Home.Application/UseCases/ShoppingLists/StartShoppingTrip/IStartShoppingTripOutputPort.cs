using Home.Domain.Entities;

namespace Home.Application.UseCases.ShoppingLists.StartShoppingTrip;

public interface IStartShoppingTripOutputPort
{

    #region Methods

    Task PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken);
    Task PresentShoppingTripAsync(ShoppingTrip shoppingTrip, CancellationToken cancellationToken);

    #endregion Methods

}
