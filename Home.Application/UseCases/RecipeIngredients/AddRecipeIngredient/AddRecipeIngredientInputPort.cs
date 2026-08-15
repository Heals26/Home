using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.RecipeIngredients.AddRecipeIngredient;

public record AddRecipeIngredientInputPort(
    decimal? Amount,
    string Name,
    long RecipeID,
    long? Unit)
    : IInputPort<IAddRecipeIngredientOutputPort>;
