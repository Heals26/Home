using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.ShoppingCategories.CreateShoppingCategory;

public interface ICreateShoppingCategoryOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentShoppingCategoryCreatedAsync(long shoppingCategoryID, CancellationToken cancellationToken);
    Task PresentShoppingCategoryNameConflictAsync(string name, CancellationToken cancellationToken);

    #endregion Methods

}
