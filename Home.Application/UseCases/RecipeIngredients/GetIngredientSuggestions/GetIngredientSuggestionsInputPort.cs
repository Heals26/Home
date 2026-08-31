using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.RecipeIngredients.GetIngredientSuggestions;

public record GetIngredientSuggestionsInputPort() : IInputPort<IGetIngredientSuggestionsOutputPort>;
