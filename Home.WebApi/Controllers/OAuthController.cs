using Home.Application.Infrastructure.Values;
using Home.Application.UseCases.OAuth.CreatePasswordGrant;
using Home.Application.UseCases.OAuth.CreateRefreshGrant;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Presenters.OAuth.CreatePasswordGrant;
using Home.WebApi.Presenters.OAuth.CreateRefreshGrant;
using Home.WebApi.UseCases.OAuth;
using Home.WebApi.UseCases.OAuth.CreatePasswordGrant;
using Home.WebApi.UseCases.OAuth.CreateRefreshGrant;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
public class OAuthController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreatePasswordGrantApiResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<CreateRefreshGrantApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOAuthToken(
        [FromServices] CreatePasswordGrantPresenter passwordPresenter,
        [FromServices] CreateRefreshGrantPresenter refreshPresenter,
        [FromForm] OAuthApiRequest apiRequest,
        CancellationToken cancellationToken)
    {
        switch (apiRequest.GrantType)
        {
            case string _GrantType when _GrantType == OAuthValues.GrantTypePassword.Name:
                {
                    await this.Pipeline.InvokeAsync(this.Mapper.Map<CreatePasswordGrantInputPort>(apiRequest), passwordPresenter, this.ServiceFactory, cancellationToken);

                    return passwordPresenter.Result;
                }
            case string _GrantType when _GrantType == OAuthValues.GrantTypeRefresh.Name:
                {
                    await this.Pipeline.InvokeAsync(this.Mapper.Map<CreateRefreshGrantInputPort>(apiRequest), refreshPresenter, this.ServiceFactory, cancellationToken);

                    return refreshPresenter.Result;
                }
        }

        return this.BadRequest();
    }

    #endregion Methods

}
