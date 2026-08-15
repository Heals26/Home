using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.MealSlots.UpdateMealSlot;

public interface IUpdateMealSlotOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentMealSlotNameConflictAsync(string name, CancellationToken cancellationToken);
    Task PresentMealSlotNoContentAsync(CancellationToken cancellationToken);
    Task PresentMealSlotNotFoundAsync(long mealSlotID, CancellationToken cancellationToken);

    #endregion Methods

}
