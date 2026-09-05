using Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;
using Home.WebUI.DataAccess.OAuth.CreateRefreshGrant;

namespace Home.WebUI.Infrastructure.Services.Security;

public interface IOAuthClient
{

    #region Methods

    Task<TokenGrantResult<CreateRefreshGrantWebAppResponse>> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    /// <summary>
    /// Exchanges a username and password for a token. The outcome distinguishes the API being
    /// unreachable, this installation's own credentials being refused, and the person's being
    /// refused, because the sign-in page has to say something different about each.
    /// </summary>
    Task<TokenGrantResult<CreatePasswordGrantWebAppResponse>> TryPasswordGrantAsync(string username, string password, CancellationToken cancellationToken);

    #endregion Methods

}
