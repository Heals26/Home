using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.RecipeIngredients.RemoveRecipeIngredient;

public record RemoveRecipeIngredientInputPort(long IngredientID, long RecipeID) : IInputPort<IRemoveRecipeIngredientOutputPort>;
