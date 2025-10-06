using Home.Domain.Entities;
using System.Security.Claims;

namespace Home.Application.Services.Security;

public interface IAuthorisationService
{

    #region Properties

    ClaimsPrincipal User { get; }

    #endregion Properties

    #region Methods

    AuthenticationMetadata GetAuthenticationMetadata();
    User GetUser();

    #endregion Methods

}
