using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.RecipeIngredients.UpdateRecipeIngredient;

public interface IUpdateRecipeIngredientOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentRecipeIngredientNoContentAsync(CancellationToken cancellationToken);
    Task PresentRecipeIngredientNotFoundAsync(long ingredientID, CancellationToken cancellationToken);

    #endregion Methods

}
