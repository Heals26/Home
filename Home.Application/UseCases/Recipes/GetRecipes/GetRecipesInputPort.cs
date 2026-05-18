using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Recipes.GetRecipes;

public record GetRecipesInputPort() : IInputPort<IGetRecipesOutputPort>;
