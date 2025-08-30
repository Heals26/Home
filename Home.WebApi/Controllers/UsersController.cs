using Home.Application.UseCases.Users.CreateUser;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Presenters.Users;
using Home.WebApi.UseCases.Users.CreateUser;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Route("api/[controller]")]
public class UsersController : BaseController
{

    #region Methods

    [Version1]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromServices] CreateUserPresenter presenter, [FromBody] CreateUserApiRequest body, CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(this.Mapper.Map<CreateUserInputPort>(body), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
