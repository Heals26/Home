using Home.Application.Services.Security;
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
            UserID = long.Parse(((IAuthorisationService)this).User.FindFirst(nameof(AuthenticationMetadata.UserID))?.Value),
            ClientApplicationID = long.Parse(((IAuthorisationService)this).User.FindFirst(nameof(AuthenticationMetadata.ClientApplicationID))?.Value),
            Scopes = ((IAuthorisationService)this).User.FindFirst(nameof(AuthenticationMetadata.ClientApplicationID))?.Value
        };

    #endregion Methods

}
