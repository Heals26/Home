using Home.Application.UseCases.ShoppingCarts.CreateShoppingCart;
using Home.Application.UseCases.ShoppingCarts.DeleteShoppingCart;
using Home.Application.UseCases.ShoppingCarts.GetShoppingCart;
using Home.Application.UseCases.ShoppingCarts.UpdateShoppingCart;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.ShoppingCarts.CreateShoppingCart;
using Home.WebApi.Presenters.ShoppingCarts.DeleteShoppingCart;
using Home.WebApi.Presenters.ShoppingCarts.GetShoppingCart;
using Home.WebApi.Presenters.ShoppingCarts.UpdateShoppingCart;
using Home.WebApi.UseCases.ShoppingCarts.CreateShoppingCart;
using Home.WebApi.UseCases.ShoppingCarts.GetShoppingCart;
using Home.WebApi.UseCases.ShoppingCarts.UpdateShoppingCart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class ShoppingCartController : BaseController
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
        await this.Pipeline.InvokeAsync(new CreateShoppingCartInputPort() { Name = request.Name }, presenter, this.ServiceFactory, cancellationToken);

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
        await this.Pipeline.InvokeAsync(new DeleteShoppingCartInputPort() { ShoppingCartID = shoppingCartID }, presenter, this.ServiceFactory, cancellationToken);

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
        await this.Pipeline.InvokeAsync(new GetShoppingCartInputPort() { ShoppingCartID = shoppingCartID }, presenter, this.ServiceFactory, cancellationToken);

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
        await this.Pipeline.InvokeAsync(new UpdateShoppingCartInputPort()
        {
            ShoppingCartID = shoppingCartID,
            Name = request.Name
        }, presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
