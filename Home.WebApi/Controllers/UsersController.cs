using Home.Application.UseCases.Users.CreateUser;
using Home.Application.UseCases.Users.GetUser;
using Home.Application.UseCases.Users.UpdateUser;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Users.CreateUser;
using Home.WebApi.Presenters.Users.GetUser;
using Home.WebApi.Presenters.Users.UpdateUser;
using Home.WebApi.UseCases.Users.CreateUser;
using Home.WebApi.UseCases.Users.GetUser;
using Home.WebApi.UseCases.Users.UpdateUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class UsersController : BaseController
{

    #region Methods

    [Version1]
    [HttpPost]
    [ProducesResponseType<CreateUserApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUser(
        [FromServices] CreateUserPresenter presenter,
        [FromBody] CreateUserApiRequest body,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(this.Mapper.Map<CreateUserInputPort>(body), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpGet("{userID}")]
    [ProducesResponseType<GetUserApiRequest>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUser(
        [FromServices] GetUserPresenter presenter,
        [FromRoute] long userID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetUserInputPort() { UserID = userID }, presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpPatch("{userID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUser(
        [FromServices] UpdateUserPresenter presenter,
        [FromRoute] long userID,
        [FromBody] UpdateUserApiRequest body,
        CancellationToken cancellationToken)
    {
        var _InputPort = this.Mapper.Map<UpdateUserInputPort>(body);
        _InputPort.UserID = userID;

        await this.Pipeline.InvokeAsync(_InputPort, presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
