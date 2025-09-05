using Home.Application.Services.User;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Home.Application.Infrastructure.Users;

public class AuthorisationService(IHttpContextAccessor httpContextAccessor) : IAuthorisationService
{

    #region Properties

    ClaimsPrincipal IAuthorisationService.User => httpContextAccessor.HttpContext.User;

    #endregion Properties

    #region Methods

    AuthenticationMetadata IAuthorisationService.GetAuthenticationMetadata()
        => new()
        {
            AccountID = long.Parse(((IAuthorisationService)this).User.FindFirst(nameof(AuthenticationMetadata.AccountID))?.Value),
            ClientApplicationID = long.Parse(((IAuthorisationService)this).User.FindFirst(nameof(AuthenticationMetadata.ClientApplicationID))?.Value),
            Scopes = ((IAuthorisationService)this).User.FindFirst(nameof(AuthenticationMetadata.ClientApplicationID))?.Value
        };

    #endregion Methods

}
