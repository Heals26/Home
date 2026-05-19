using Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;
using Home.WebUI.DataAccess.OAuth.CreateRefreshGrant;
using Home.WebUI.ViewModels.OAuth;

namespace Home.WebUI.Infrastructure.Services.Security;

public interface IAuthorisationService
{

    #region Methods

    Task<OAuthViewModel?> GetTokenAsync();
    Task<bool> IsTokenExpiredAsync();
    ValueTask SignOutAsync();
    Task<bool> TryRefreshAsync(CreateRefreshGrantWebAppResponse response, CancellationToken cancellationToken);
    Task<bool> TrySignInAsync(CreatePasswordGrantWebAppRequest request, CreatePasswordGrantWebAppResponse response, CancellationToken cancellationToken);

    #endregion Methods

}
