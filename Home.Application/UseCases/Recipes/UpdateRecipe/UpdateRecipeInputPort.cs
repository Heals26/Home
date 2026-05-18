using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.Recipes.UpdateRecipe;

public record UpdateRecipeInputPort(
    long RecipeID,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<string> Url)
    : IInputPort<IUpdateRecipeOutputPort>;
