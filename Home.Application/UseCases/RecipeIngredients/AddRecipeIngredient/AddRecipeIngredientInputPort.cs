using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.RecipeIngredients.AddRecipeIngredient;

public record AddRecipeIngredientInputPort(
    string Name,
    decimal? Quantity,
    long RecipeID,
    decimal? Volume,
    decimal? Weight)
    : IInputPort<IAddRecipeIngredientOutputPort>;
