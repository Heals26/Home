using Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;
using Home.WebUI.DataAccess.OAuth.CreateRefreshGrant;
using Home.WebUI.DataAccess.OAuth.Models;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.HttpClients;
using Home.WebUI.Infrastructure.Services.Security;
using Home.WebUI.Infrastructure.Values;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Home.WebUI.Infrastructure.Security;

/// <summary>
/// The token endpoint, and nothing else. Split out from <see cref="HttpClients.HomeHttpClient"/>
/// so the sign-in endpoint and the circuit's session can both reach it without either depending on
/// the other — the cycle that used to force a service-locator lookup at startup.
/// </summary>
public class OAuthClient(IConfiguration configurationManager, HttpClient httpClient) : IOAuthClient
{

    #region Methods

    /// <summary>
    /// The error code out of a refusal, or null when the body is not one. An endpoint that answers
    /// 401 with something else entirely, such as a reverse proxy, must not be read as a verdict on
    /// anybody's credentials.
    /// </summary>
    private static string? ReadErrorCode(string body)
    {
        try
        {
            return JsonSerializer.Deserialize<OAuthErrorWebAppResponse>(body, JsonOptions.DefaultOptions)?.Error;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private string GetClientCredentials()
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{configurationManager.GetValue<string>("OAuth:AccessToken:AccessToken")!}:{configurationManager.GetValue<string>("OAuth:AccessToken:ClientSecret")!}"));

    async Task<TokenGrantResult<CreatePasswordGrantWebAppResponse>> IOAuthClient.TryPasswordGrantAsync(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        var _Request = new CreatePasswordGrantWebAppRequest()
        {
            ClientID = configurationManager.GetValue<long>("OAuth:AccessToken:ClientID")!,
            ClientSecret = configurationManager.GetValue<string>("OAuth:AccessToken:ClientSecret")!,
            GrantType = configurationManager.GetValue<string>("OAuth:AccessToken:GrantType")!,
            Password = password,
            Scope = configurationManager.GetValue<string>("OAuth:AccessToken:Scope")!,
            Username = username
        };

        return await this.SendAsync<CreatePasswordGrantWebAppRequest, CreatePasswordGrantWebAppResponse>(_Request, cancellationToken);
    }

    async Task<TokenGrantResult<CreateRefreshGrantWebAppResponse>> IOAuthClient.RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var _Request = new CreateRefreshGrantWebAppRequest()
        {
            ClientID = configurationManager.GetValue<long>("OAuth:AccessToken:ClientID")!,
            ClientSecret = configurationManager.GetValue<string>("OAuth:AccessToken:ClientSecret")!,
            GrantType = AuthorisationValues.RefreshGrantType,
            RefreshToken = refreshToken
        };

        return await this.SendAsync<CreateRefreshGrantWebAppRequest, CreateRefreshGrantWebAppResponse>(_Request, cancellationToken);
    }

    /// <summary>
    /// Only the token endpoint answering Unauthorized or BadRequest says the session is over.
    /// A 500, a proxy error, or an API that is simply not up yet says nothing about it. The
    /// request stays generically typed because the form content reflects over TRequest — as
    /// <c>object</c> it would serialise to an empty form.
    /// </summary>
    private async Task<TokenGrantResult<TResponse>> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TResponse : class
    {
        try
        {
            var _Provider = ApiProvider.GetOAuthToken();

            var _Message = new HttpRequestMessage(_Provider.HttpMethod, _Provider.Uri)
            {
                Content = _Provider.RouteType.GetHttpRequestMessage(request)
            };

            _Message.Headers.Add("api-version", _Provider.Version);
            _Message.Headers.Authorization = new AuthenticationHeaderValue("Basic", this.GetClientCredentials());

            var _HttpResponse = await httpClient.SendAsync(_Message, HttpCompletionOption.ResponseContentRead, cancellationToken);
            var _Body = await _HttpResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!_HttpResponse.IsSuccessStatusCode)
            {
                if (_HttpResponse.StatusCode is not (HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest))
                    return new(TokenRefreshOutcome.Unavailable, null);

                // The endpoint names which of its own preconditions failed, so a refusal of this
                // installation's credentials can be told apart from a refusal of the person's.
                return new(ReadErrorCode(_Body) == AuthorisationValues.InvalidClientError
                    ? TokenRefreshOutcome.ClientRejected
                    : TokenRefreshOutcome.Rejected, null);
            }

            var _Token = JsonSerializer.Deserialize<TResponse>(_Body, JsonOptions.DefaultOptions);

            return _Token == null
                ? new(TokenRefreshOutcome.Unavailable, null)
                : new(TokenRefreshOutcome.Refreshed, _Token);
        }
        catch (HttpRequestException)
        {
            return new(TokenRefreshOutcome.Unavailable, null);
        }
        catch (OperationCanceledException)
        {
            // Covers TaskCanceledException, which HttpClient raises for both a timeout and a
            // cancelled token. Neither says anything about whether the session is still good.
            return new(TokenRefreshOutcome.Unavailable, null);
        }
        catch (JsonException)
        {
            return new(TokenRefreshOutcome.Unavailable, null);
        }
    }

    #endregion Methods

}
