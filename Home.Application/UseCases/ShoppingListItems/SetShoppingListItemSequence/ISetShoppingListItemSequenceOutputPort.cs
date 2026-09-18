namespace Home.Application.UseCases.ShoppingListItems.SetShoppingListItemSequence;

public interface ISetShoppingListItemSequenceOutputPort
{

    #region Methods

    Task PresentShoppingListItemNotFoundAsync(long shoppingListItemID, CancellationToken cancellationToken);
    Task PresentShoppingListItemSequenceSetAsync(CancellationToken cancellationToken);

    #endregion Methods

}
