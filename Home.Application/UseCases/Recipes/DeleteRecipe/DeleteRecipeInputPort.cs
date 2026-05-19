using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Recipes.DeleteRecipe;

public record DeleteRecipeInputPort(long RecipeID) : IInputPort<IDeleteRecipeOutputPort>;
