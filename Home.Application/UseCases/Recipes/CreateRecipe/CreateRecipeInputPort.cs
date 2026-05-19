using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Recipes.CreateRecipe;

public record CreateRecipeInputPort(string Name, string Url) : IInputPort<ICreateRecipeOutputPort>;
