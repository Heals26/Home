using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace Home.WebUI.Infrastructure.Security;

public class TokenDelegatingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{

    #region Methods

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var _AccessToken = await httpContextAccessor.HttpContext?.GetTokenAsync("access_token")!;
        if (!string.IsNullOrEmpty(_AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }

    #endregion Methods

}
