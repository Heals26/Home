using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.Recipes.UpdateRecipe;

public record UpdateRecipeApiRequest(
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<string> Url);
