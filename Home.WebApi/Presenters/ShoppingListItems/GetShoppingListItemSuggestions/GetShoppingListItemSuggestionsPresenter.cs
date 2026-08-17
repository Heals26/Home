using AutoMapper;
using Home.Application.UseCases.ShoppingListItems.GetShoppingListItemSuggestions;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItemSuggestions;

namespace Home.WebApi.Presenters.ShoppingListItems.GetShoppingListItemSuggestions;

public class GetShoppingListItemSuggestionsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetShoppingListItemSuggestionsOutputPort
{

    #region Methods

    Task IGetShoppingListItemSuggestionsOutputPort.PresentShoppingListItemSuggestionsAsync(IEnumerable<ShoppingListItemSuggestion> suggestions, CancellationToken cancellationToken)
        => this.OkAsync(mapper.Map<GetShoppingListItemSuggestionsApiResponse>(suggestions), cancellationToken);

    #endregion Methods

}
