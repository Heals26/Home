
using Home.Application.UseCases.ShoppingCartItems.CreateShoppingCartItem;
using Home.Application.UseCases.ShoppingCartItems.DeleteShoppingCartItem;
using Home.Application.UseCases.ShoppingCartItems.GetShoppingCartItem;
using Home.Application.UseCases.ShoppingCartItems.UpdateShoppingCartItem;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.ShoppingCartItems.CreateShoppingCartItem;
using Home.WebApi.Presenters.ShoppingCartItems.DeleteShoppingCartItem;
using Home.WebApi.Presenters.ShoppingCartItems.GetShoppingCart;
using Home.WebApi.Presenters.ShoppingCartItems.UpdateShoppingCartItem;
using Home.WebApi.UseCases.ShoppingCartItems.CreateShoppingCartItem;
using Home.WebApi.UseCases.ShoppingCartItems.GetShoppingCartItem;
using Home.WebApi.UseCases.ShoppingCartItems.UpdateShoppingCartItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class ShoppingCartItemsController : BaseController
{

    #region Methods

    [Version1]
    [HttpPost]
    [ProducesResponseType<CreateShoppingCartItemApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateShoppingCartItem(
        [FromServices] CreateShoppingCartItemPresenter presenter,
        [FromBody] CreateShoppingCartItemApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateShoppingCartItemInputPort(request.Cost, request.InBasket, request.Name, request.Quantity, request.ShoppingCartID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpDelete("{shoppingCartItemID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteShoppingCartItem(
        [FromServices] DeleteShoppingCartItemPresenter presenter,
        [FromRoute] long shoppingCartItemID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteShoppingCartItemInputPort(shoppingCartItemID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpGet("{shoppingCartItemID}")]
    [ProducesResponseType<GetShoppingCartItemApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShoppingCartItem(
        [FromServices] GetShoppingCartItemPresenter presenter,
        [FromRoute] long shoppingCartID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetShoppingCartItemInputPort(shoppingCartID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpPatch("{shoppingCartItemID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateShoppingCartItem(
        [FromServices] UpdateShoppingCartItemPresenter presenter,
        [FromRoute] long shoppingCartItemID,
        [FromBody] UpdateShoppingCartItemApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateShoppingCartItemInputPort(
            request.Cost,
            request.InBasket,
            request.Name,
            request.Quantity,
            shoppingCartItemID,
            request.Sequence
            ), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
