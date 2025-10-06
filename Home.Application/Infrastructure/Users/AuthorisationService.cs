using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Home.Application.Infrastructure.Users;

public class AuthorisationService(IHttpContextAccessor httpContextAccessor, IPersistenceContext persistenceContext) : IAuthorisationService
{

    #region Properties

    ClaimsPrincipal IAuthorisationService.User => httpContextAccessor.HttpContext.User;

    #endregion Properties

    #region Methods

    Services.Security.AuthenticationMetadata IAuthorisationService.GetAuthenticationMetadata()
        => new()
        {
            UserID = long.Parse(((IAuthorisationService)this).User.FindFirst(nameof(AuthenticationMetadata.UserID))?.Value),
            ClientApplicationID = long.Parse(((IAuthorisationService)this).User.FindFirst(nameof(AuthenticationMetadata.ClientApplicationID))?.Value),
            ClientName = ((IAuthorisationService)this).User.FindFirst(nameof(AuthenticationMetadata.ClientName))?.Value,
            Scopes = ((IAuthorisationService)this).User.FindFirst(nameof(AuthenticationMetadata.Scopes))?.Value
        };

    User IAuthorisationService.GetUser()
    {
        var _UserID = ((IAuthorisationService)this).User.FindFirst(nameof(Services.Security.AuthenticationMetadata.UserID));
        return persistenceContext.Find<User>(_UserID!);
    }

    #endregion Methods

}
