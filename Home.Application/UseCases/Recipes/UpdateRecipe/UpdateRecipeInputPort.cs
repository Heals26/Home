using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.Recipes.UpdateRecipe;

public record UpdateRecipeInputPort(
    PropertyChangeTracker<long?> Complexity,
    PropertyChangeTracker<int?> CookMinutes,
    PropertyChangeTracker<string> ImageUrl,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<int?> PrepMinutes,
    long RecipeID,
    PropertyChangeTracker<int?> Servings,
    PropertyChangeTracker<string> Url)
    : IInputPort<IUpdateRecipeOutputPort>;
