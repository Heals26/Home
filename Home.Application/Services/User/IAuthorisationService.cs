using System.Security.Claims;

namespace Home.Application.Services.User;

public interface IAuthorisationService
{

    #region Properties

    ClaimsPrincipal User { get; }

    #endregion Properties

    #region Methods

    AuthenticationMetadata GetAuthenticationMetadata();

    #endregion Methods

}
