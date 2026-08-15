using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.RecipeIngredients.UpdateRecipeIngredient;

public record UpdateRecipeIngredientApiRequest(
    PropertyChangeTracker<decimal?> Amount,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<long?> Unit);
