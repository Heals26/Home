using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;
using Home.Domain.Entities;

namespace Home.Application.UseCases.MealPlanEntries.GetMealPlanEntries;

public interface IGetMealPlanEntriesOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentMealPlanEntriesAsync(IEnumerable<MealPlanEntry> mealPlanEntries, CancellationToken cancellationToken);

    #endregion Methods

}
