using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.RecipeIngredients.SetRecipeIngredientSequence;

public record SetRecipeIngredientSequenceInputPort(long IngredientID, long RecipeID, long Sequence) : IInputPort<ISetRecipeIngredientSequenceOutputPort>;
