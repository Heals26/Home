using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.RecipeIngredients.UpdateRecipeIngredient;

public record UpdateRecipeIngredientInputPort(
    long IngredientID,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<decimal?> Quantity,
    PropertyChangeTracker<decimal?> Volume,
    PropertyChangeTracker<decimal?> Weight)
    : IInputPort<IUpdateRecipeIngredientOutputPort>;
