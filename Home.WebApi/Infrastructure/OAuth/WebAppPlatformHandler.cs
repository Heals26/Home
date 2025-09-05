using Home.Application.Services.Persistence;
using Home.Application.Services.User;
using Home.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Home.WebApi.Infrastructure.OAuth;

public class WebAppPlatformRequirement : IAuthorizationRequirement { }

public class WebAppPlatformHandler(
    IAuthorisationService authorisationService,
    IHttpContextAccessor httpContextAccessor,
    IServiceScopeFactory serviceScopeFactory) : AuthorizationHandler<WebAppPlatformRequirement>
{

    #region Methods

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, WebAppPlatformRequirement requirement)
    {
        var _Scope = serviceScopeFactory.CreateScope();
        var _PersistenceContext = _Scope.ServiceProvider.GetRequiredService<IPersistenceContext>();

        var _ClientApplication = _PersistenceContext.GetEntities<ClientApplication>().ToList();

        var _AuthenticationMetadata = authorisationService.GetAuthenticationMetadata();

        if (_ClientApplication.Any(ca => ca.ClientApplicationID == _AuthenticationMetadata.ClientApplicationID)
            context.Succeed(requirement);
        else
        {
            httpContextAccessor.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Fail(new(this, "Invalid Platform Requirement"));
        }

        return Task.CompletedTask;
    }

    #endregion Methods

}
