using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.MealPlanEntries.UpdateMealPlanEntry;

public interface IUpdateMealPlanEntryOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentMealPlanEntryNoContentAsync(CancellationToken cancellationToken);
    Task PresentMealPlanEntryNotFoundAsync(long mealPlanEntryID, CancellationToken cancellationToken);

    #endregion Methods

}
