using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.MealPlanEntries.CreateMealPlanEntry;

public interface ICreateMealPlanEntryOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentMealPlanEntryCreatedAsync(long mealPlanEntryID, CancellationToken cancellationToken);
    Task PresentMealSlotNotFoundAsync(long mealSlotID, CancellationToken cancellationToken);

    /// <summary>
    /// Neither a recipe nor a title arrived, so there is nothing to put on the day.
    /// </summary>
    Task PresentNothingToPlanAsync(CancellationToken cancellationToken);

    Task PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken);

    #endregion Methods

}
