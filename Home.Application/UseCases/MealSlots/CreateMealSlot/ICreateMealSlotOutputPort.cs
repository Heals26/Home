using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.MealSlots.CreateMealSlot;

public interface ICreateMealSlotOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentMealSlotCreatedAsync(long mealSlotID, CancellationToken cancellationToken);
    Task PresentMealSlotNameConflictAsync(string name, CancellationToken cancellationToken);

    #endregion Methods

}
