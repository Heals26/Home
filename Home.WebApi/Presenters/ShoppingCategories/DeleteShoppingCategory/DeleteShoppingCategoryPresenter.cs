using AutoMapper;
using Home.Application.UseCases.ShoppingCategories.DeleteShoppingCategory;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingCategories.DeleteShoppingCategory;

public class DeleteShoppingCategoryPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteShoppingCategoryOutputPort
{

    #region Methods

    Task IDeleteShoppingCategoryOutputPort.PresentShoppingCategoryDeletedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IDeleteShoppingCategoryOutputPort.PresentShoppingCategoryNotFoundAsync(long shoppingCategoryID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping Category {shoppingCategoryID} Not Found", cancellationToken);

    #endregion Methods

}
