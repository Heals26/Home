using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.WebApi.UseCases.RecipeIngredients.UpdateRecipeIngredient;

public record UpdateRecipeIngredientApiRequest(
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<decimal?> Quantity,
    PropertyChangeTracker<decimal?> Volume,
    PropertyChangeTracker<decimal?> Weight);
