using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Recipes.CreateRecipe;

public record CreateRecipeInputPort(
    long? Complexity,
    int? CookMinutes,
    string ImageUrl,
    string Name,
    int? PrepMinutes,
    int? Servings,
    string Url)
    : IInputPort<ICreateRecipeOutputPort>;
