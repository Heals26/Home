using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Recipes.GetRecipe;

public record GetRecipeInputPort(long RecipeID) : IInputPort<IGetRecipeOutputPort>;
