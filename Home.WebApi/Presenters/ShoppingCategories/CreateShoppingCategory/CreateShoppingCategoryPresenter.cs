using AutoMapper;
using Home.Application.UseCases.ShoppingCategories.CreateShoppingCategory;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingCategories.CreateShoppingCategory;

namespace Home.WebApi.Presenters.ShoppingCategories.CreateShoppingCategory;

public class CreateShoppingCategoryPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateShoppingCategoryOutputPort
{

    #region Methods

    Task ICreateShoppingCategoryOutputPort.PresentShoppingCategoryCreatedAsync(long shoppingCategoryID, CancellationToken cancellationToken)
        => this.CreatedAsync(shoppingCategoryID, new CreateShoppingCategoryApiResponse() { ShoppingCategoryID = shoppingCategoryID }, cancellationToken);

    Task ICreateShoppingCategoryOutputPort.PresentShoppingCategoryNameConflictAsync(string name, CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    #endregion Methods

}
