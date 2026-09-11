using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;

namespace Home.Application.UseCases.ShoppingCategories.UpdateShoppingCategory;

public interface IUpdateShoppingCategoryOutputPort
    : IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>
{

    #region Methods

    Task PresentShoppingCategoryNameConflictAsync(string name, CancellationToken cancellationToken);
    Task PresentShoppingCategoryNoContentAsync(CancellationToken cancellationToken);
    Task PresentShoppingCategoryNotFoundAsync(long shoppingCategoryID, CancellationToken cancellationToken);

    #endregion Methods

}
