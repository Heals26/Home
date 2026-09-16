namespace Home.Application.UseCases.ShoppingLists.EndShoppingTrip;

public interface IEndShoppingTripOutputPort
{

    #region Methods

    Task PresentShoppingTripEndedNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
