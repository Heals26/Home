using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.ShoppingLists.AddMealPlanToShoppingList;

public interface IAddMealPlanToShoppingListOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentMealPlanAddedToShoppingListAsync(int recipeCount, CancellationToken cancellationToken);
    Task PresentShoppingListNotFoundAsync(long shoppingListID, CancellationToken cancellationToken);

    #endregion Methods

}
