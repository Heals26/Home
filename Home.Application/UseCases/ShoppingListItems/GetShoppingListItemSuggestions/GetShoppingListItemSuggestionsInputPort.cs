using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.ShoppingListItems.GetShoppingListItemSuggestions;

public record GetShoppingListItemSuggestionsInputPort() : IInputPort<IGetShoppingListItemSuggestionsOutputPort>;
