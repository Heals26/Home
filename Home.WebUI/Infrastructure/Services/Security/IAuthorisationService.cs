using Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;
using Home.WebUI.DataAccess.OAuth.CreateRefreshGrant;

namespace Home.WebUI.Infrastructure.Services.Security;

public interface IAuthorisationService
{

    #region Methods

    Task<bool> TryRefreshAsync(CreateRefreshGrantWebAppResponse response, CancellationToken cancellationToken);
    Task<bool> TrySignInAsync(CreatePasswordGrantWebAppRequest request, CreatePasswordGrantWebAppResponse response, CancellationToken cancellationToken);

    #endregion Methods

}
