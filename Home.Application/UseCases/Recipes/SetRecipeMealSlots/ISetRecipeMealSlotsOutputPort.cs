using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.Recipes.SetRecipeMealSlots;

public interface ISetRecipeMealSlotsOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentMealSlotNotFoundAsync(long mealSlotID, CancellationToken cancellationToken);
    Task PresentRecipeMealSlotsSetAsync(CancellationToken cancellationToken);
    Task PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken);

    #endregion Methods

}
