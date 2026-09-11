using AutoMapper;
using Home.Application.UseCases.ShoppingCategories.GetShoppingCategories;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.ShoppingCategories.GetShoppingCategories;
using Home.WebApi.UseCases.ShoppingCategories.Models;

namespace Home.WebApi.Presenters.ShoppingCategories.GetShoppingCategories;

public class GetShoppingCategoriesPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetShoppingCategoriesOutputPort
{

    #region Methods

    Task IGetShoppingCategoriesOutputPort.PresentShoppingCategoriesAsync(IEnumerable<ShoppingCategory> shoppingCategories, CancellationToken cancellationToken)
        => this.OkAsync(new GetShoppingCategoriesApiResponse()
        {
            ShoppingCategories = [.. shoppingCategories.Select(c => new ShoppingCategoryDto()
            {
                Name = c.Name,
                Sequence = c.Sequence,
                ShoppingCategoryID = c.ShoppingCategoryID
            })]
        }, cancellationToken);

    #endregion Methods

}
