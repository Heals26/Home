using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.RecipeIngredients.GetRecipeIngredient;

public record GetRecipeIngredientInputPort(long IngredientID)
    : IInputPort<IGetRecipeIngredientOutputPort>;
