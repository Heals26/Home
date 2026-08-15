using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Recipes.GetRecipes;

public record GetRecipesInputPort(long? MealSlotID) : IInputPort<IGetRecipesOutputPort>;
