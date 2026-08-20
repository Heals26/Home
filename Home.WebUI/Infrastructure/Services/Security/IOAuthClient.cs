using Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;
using Home.WebUI.DataAccess.OAuth.CreateRefreshGrant;

namespace Home.WebUI.Infrastructure.Services.Security;

public interface IOAuthClient
{

    #region Methods

    Task<TokenGrantResult<CreateRefreshGrantWebAppResponse>> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task<CreatePasswordGrantWebAppResponse?> TryPasswordGrantAsync(string username, string password, CancellationToken cancellationToken);

    #endregion Methods

}
