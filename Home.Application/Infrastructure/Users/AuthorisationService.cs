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
    {
        var _Service = (IAuthorisationService)this;
        var _UserIDValue = _Service.User.FindFirst(nameof(AuthenticationMetadata.UserID))?.Value;
        var _ClientIDValue = _Service.User.FindFirst(nameof(AuthenticationMetadata.ClientApplicationID))?.Value;

        if (_UserIDValue == null || _ClientIDValue == null)
            throw new UnauthorizedAccessException("Required authentication claims are missing.");

        return new()
        {
            UserID = long.Parse(_UserIDValue),
            ClientApplicationID = long.Parse(_ClientIDValue),
            ClientName = _Service.User.FindFirst(nameof(AuthenticationMetadata.ClientName))?.Value,
            Scopes = _Service.User.FindFirst(nameof(AuthenticationMetadata.Scopes))?.Value
        };
    }

    User IAuthorisationService.GetUser()
    {
        var _UserIDValue = ((IAuthorisationService)this).User
            .FindFirst(nameof(Services.Security.AuthenticationMetadata.UserID))?.Value;

        if (_UserIDValue == null || !long.TryParse(_UserIDValue, out var _UserID))
            throw new UnauthorizedAccessException("User identity claim is missing or invalid.");

        return persistenceContext.Find<User>(_UserID);
    }

    #endregion Methods

}
