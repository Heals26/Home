using Home.Application.UseCases.ShoppingCartItems.GetShoppingCartItems;
using Home.Application.UseCases.ShoppingCarts.CreateShoppingCart;
using Home.Application.UseCases.ShoppingCarts.DeleteShoppingCart;
using Home.Application.UseCases.ShoppingCarts.GetShoppingCart;
using Home.Application.UseCases.ShoppingCarts.GetShoppingCarts;
using Home.Application.UseCases.ShoppingCarts.UpdateShoppingCart;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.ShoppingCartItems.GetShoppingCartItems;
using Home.WebApi.Presenters.ShoppingCarts.CreateShoppingCart;
using Home.WebApi.Presenters.ShoppingCarts.DeleteShoppingCart;
using Home.WebApi.Presenters.ShoppingCarts.GetShoppingCart;
using Home.WebApi.Presenters.ShoppingCarts.GetShoppingCarts;
using Home.WebApi.Presenters.ShoppingCarts.UpdateShoppingCart;
using Home.WebApi.UseCases.ShoppingCartItems.GetShoppingCartItems;
using Home.WebApi.UseCases.ShoppingCarts.CreateShoppingCart;
using Home.WebApi.UseCases.ShoppingCarts.GetShoppingCart;
using Home.WebApi.UseCases.ShoppingCarts.GetShoppingCarts;
using Home.WebApi.UseCases.ShoppingCarts.UpdateShoppingCart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class ShoppingCartsController : BaseController
{

    #region Methods

    [Version1]
    [HttpPost]
    [ProducesResponseType<CreateShoppingCartApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateShoppingCart(
        [FromServices] CreateShoppingCartPresenter presenter,
        [FromBody] CreateShoppingCartApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateShoppingCartInputPort(request.Name), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpDelete("{shoppingCartID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteShoppingCart(
        [FromServices] DeleteShoppingCartPresenter presenter,
        [FromRoute] long shoppingCartID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteShoppingCartInputPort(shoppingCartID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpGet("{shoppingCartID}")]
    [ProducesResponseType<GetShoppingCartApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShoppingCart(
        [FromServices] GetShoppingCartPresenter presenter,
        [FromRoute] long shoppingCartID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetShoppingCartInputPort(shoppingCartID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpGet]
    [ProducesResponseType<GetShoppingCartsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShoppingCarts(
        [FromServices] GetShoppingCartsPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetShoppingCartsInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpGet("{shoppingCartID}/Items/Items")]
    [ProducesResponseType<GetShoppingCartItemsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShoppingCartItems(
        [FromServices] GetShoppingCartItemsPresenter presenter,
        [FromRoute] long shoppingCartID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetShoppingCartItemsInputPort(shoppingCartID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpPatch("{shoppingCartID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateShoppingCart(
        [FromServices] UpdateShoppingCartPresenter presenter,
        [FromRoute] long shoppingCartID,
        [FromBody] UpdateShoppingCartApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateShoppingCartInputPort(request.Name, shoppingCartID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
