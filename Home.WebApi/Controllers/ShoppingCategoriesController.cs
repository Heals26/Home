using Home.Application.UseCases.ShoppingCategories.CreateShoppingCategory;
using Home.Application.UseCases.ShoppingCategories.DeleteShoppingCategory;
using Home.Application.UseCases.ShoppingCategories.GetShoppingCategories;
using Home.Application.UseCases.ShoppingCategories.UpdateShoppingCategory;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.ShoppingCategories.CreateShoppingCategory;
using Home.WebApi.Presenters.ShoppingCategories.DeleteShoppingCategory;
using Home.WebApi.Presenters.ShoppingCategories.GetShoppingCategories;
using Home.WebApi.Presenters.ShoppingCategories.UpdateShoppingCategory;
using Home.WebApi.UseCases.ShoppingCategories.CreateShoppingCategory;
using Home.WebApi.UseCases.ShoppingCategories.GetShoppingCategories;
using Home.WebApi.UseCases.ShoppingCategories.UpdateShoppingCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class ShoppingCategoriesController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreateShoppingCategoryApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateShoppingCategory(
        [FromServices] CreateShoppingCategoryPresenter presenter,
        [FromBody] CreateShoppingCategoryApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateShoppingCategoryInputPort(request.Name), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{shoppingCategoryID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteShoppingCategory(
        [FromServices] DeleteShoppingCategoryPresenter presenter,
        [FromRoute] long shoppingCategoryID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteShoppingCategoryInputPort(shoppingCategoryID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet]
    [ProducesResponseType<GetShoppingCategoriesApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShoppingCategories(
        [FromServices] GetShoppingCategoriesPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetShoppingCategoriesInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{shoppingCategoryID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateShoppingCategory(
        [FromServices] UpdateShoppingCategoryPresenter presenter,
        [FromRoute] long shoppingCategoryID,
        [FromBody] UpdateShoppingCategoryApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateShoppingCategoryInputPort(request.Name, request.Sequence, shoppingCategoryID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
