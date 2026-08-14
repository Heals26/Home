using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Recipes.ImportRecipe;

public record ImportRecipeInputPort(string Url) : IInputPort<IImportRecipeOutputPort>;
