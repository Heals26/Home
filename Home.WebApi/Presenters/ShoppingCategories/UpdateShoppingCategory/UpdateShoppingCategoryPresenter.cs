using AutoMapper;
using Home.Application.UseCases.ShoppingCategories.UpdateShoppingCategory;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.ShoppingCategories.UpdateShoppingCategory;

public class UpdateShoppingCategoryPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateShoppingCategoryOutputPort
{

    #region Methods

    Task IUpdateShoppingCategoryOutputPort.PresentShoppingCategoryNameConflictAsync(string name, CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    Task IUpdateShoppingCategoryOutputPort.PresentShoppingCategoryNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUpdateShoppingCategoryOutputPort.PresentShoppingCategoryNotFoundAsync(long shoppingCategoryID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Shopping Category {shoppingCategoryID} Not Found", cancellationToken);

    #endregion Methods

}
