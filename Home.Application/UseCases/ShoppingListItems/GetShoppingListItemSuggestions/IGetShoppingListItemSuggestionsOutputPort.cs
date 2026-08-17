namespace Home.Application.UseCases.ShoppingListItems.GetShoppingListItemSuggestions;

public interface IGetShoppingListItemSuggestionsOutputPort
{

    #region Methods

    Task PresentShoppingListItemSuggestionsAsync(IEnumerable<ShoppingListItemSuggestion> suggestions, CancellationToken cancellationToken);

    #endregion Methods

}
