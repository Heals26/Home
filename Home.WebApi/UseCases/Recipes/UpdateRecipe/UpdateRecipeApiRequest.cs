using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.Recipes.UpdateRecipe;

public record UpdateRecipeApiRequest(
    PropertyChangeTracker<long?> Complexity,
    PropertyChangeTracker<int?> CookMinutes,
    PropertyChangeTracker<string> ImageUrl,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<int?> PrepMinutes,
    PropertyChangeTracker<int?> Servings,
    PropertyChangeTracker<string> Url);
