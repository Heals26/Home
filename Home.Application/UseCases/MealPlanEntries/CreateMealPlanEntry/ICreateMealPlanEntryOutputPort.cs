using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.MealPlanEntries.CreateMealPlanEntry;

public interface ICreateMealPlanEntryOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentMealPlanEntryCreatedAsync(long mealPlanEntryID, CancellationToken cancellationToken);
    Task PresentMealSlotNotFoundAsync(long mealSlotID, CancellationToken cancellationToken);
    Task PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken);

    #endregion Methods

}
