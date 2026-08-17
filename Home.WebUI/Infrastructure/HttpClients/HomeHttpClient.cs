using Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;
using Home.WebUI.DataAccess.OAuth.CreateRefreshGrant;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.ApiProviders.Helpers;
using Home.WebUI.Infrastructure.Services.HttpClients;
using Home.WebUI.Infrastructure.Services.Security;
using Home.WebUI.Infrastructure.Values;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Home.WebUI.Infrastructure.HttpClients;

public class HomeHttpClient(
    IAuthorisationService authorisationService,
    IConfiguration configurationManager,
    HttpClient httpClient,
    ILoginThrottle loginThrottle)
    : IHomeHttpClient
{

    #region Fields

    /// <summary>
    /// One refresh at a time. The dashboard fires six loads in parallel and every one of them can
    /// come back 401; without this, five would race for a refresh token the first has already
    /// spent, and the API would refuse them all. Registered scoped so a circuit shares one gate.
    /// </summary>
    private readonly SemaphoreSlim m_RefreshGate = new(1, 1);

    #endregion Fields

    #region Methods

    public Task<TResponse?> SendRequestAsync<TRequest, TResponse>(
        TRequest request,
        ApiProviderHelper apiProvider,
        Action<ValidationProblemDetails> errors,
        CancellationToken cancellationToken)
        => this.SendAsync<TRequest, TResponse>(request, apiProvider, errors, true, cancellationToken);

    private static HttpRequestMessage BuildMessage<TRequest>(TRequest request, ApiProviderHelper apiProvider)
    {
        var _Message = new HttpRequestMessage(apiProvider.HttpMethod, apiProvider.Uri)
        {
            Content = apiProvider.RouteType.GetHttpRequestMessage(request)
        };
        _Message.Headers.Add("api-version", apiProvider.Version);
        return _Message;
    }

    private ValidationProblemDetails ConvertProblemDetailsToValidationProblemDetails(ProblemDetails problemDetails)
        => problemDetails is ValidationProblemDetails _ValidationProblemDetails
            ? _ValidationProblemDetails
            : new ValidationProblemDetails()
            {
                Title = problemDetails.Title,
                Status = problemDetails.Status,
                Detail = problemDetails.Detail,
                Instance = problemDetails.Instance,
                Type = problemDetails.Type,
                Errors = new Dictionary<string, string[]>()
            };

    private async Task<TResponse?> SendAsync<TRequest, TResponse>(
        TRequest request,
        ApiProviderHelper apiProvider,
        Action<ValidationProblemDetails> errors,
        bool allowRefresh,
        CancellationToken cancellationToken)
    {
        try
        {
            var _Token = await authorisationService.GetTokenAsync();
            var _IsAuthenticated = _Token != null && !string.IsNullOrEmpty(_Token.AccessToken);
            var _IsLoginEndpoint = apiProvider.Uri.Contains(ApiProvider.GetOAuthToken().Uri, StringComparison.OrdinalIgnoreCase);

            // Build a fresh HttpRequestMessage per attempt — they cannot be reused across calls
            var _HttpRequestMessage = BuildMessage(request, apiProvider);

            // The token endpoint authenticates the client application, never the user, so it is
            // checked ahead of the bearer branch — a leftover token must not turn a sign-in into
            // a request the API answers 401 to.
            if (_IsLoginEndpoint)
            {
                _HttpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", this.GetClientCredentials());
            }
            else if (_IsAuthenticated)
            {
                _HttpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _Token!.AccessToken);
            }
            else if (!apiProvider.AllowsAnonymous)
            {
                errors.Invoke(new()
                {
                    Title = "Not signed in.",
                    Status = (int)HttpStatusCode.Unauthorized,
                    Detail = "Please sign in to access this resource.",
                    Errors = new Dictionary<string, string[]>()
                });
                return default;
            }

            var _HttpResponse = await httpClient.SendAsync(_HttpRequestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken);
            var _Content = await _HttpResponse.Content.ReadAsStringAsync(cancellationToken);

            if (_HttpResponse.IsSuccessStatusCode)
            {
                return typeof(TResponse) == typeof(bool)
                    ? (TResponse)(object)true
                    : JsonSerializer.Deserialize<TResponse>(_Content, JsonOptions.DefaultOptions);
            }

            switch (_HttpResponse.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    if (string.IsNullOrWhiteSpace(_Content))
                        errors.Invoke(new()
                        {
                            Title = "Not found.",
                            Status = (int)HttpStatusCode.NotFound,
                            Detail = "The requested resource could not be found. Ensure the API is running and the endpoint is correct.",
                            Errors = new Dictionary<string, string[]>()
                        });
                    else
                        errors.Invoke(this.ConvertProblemDetailsToValidationProblemDetails(JsonSerializer.Deserialize<ProblemDetails>(_Content, JsonOptions.DefaultOptions)!));
                    return default;
                case HttpStatusCode.BadRequest:
                    errors.Invoke(this.ConvertProblemDetailsToValidationProblemDetails(JsonSerializer.Deserialize<ProblemDetails>(_Content, JsonOptions.DefaultOptions)!));
                    return default;
                case HttpStatusCode.Unauthorized:
                    if (_IsLoginEndpoint || !allowRefresh)
                    {
                        errors.Invoke(new ValidationProblemDetails()
                        {
                            Title = "Not signed in.",
                            Status = (int)HttpStatusCode.Unauthorized,
                            Detail = "Your sign-in was not accepted. Please sign in again.",
                            Errors = new Dictionary<string, string[]>()
                        });
                        return default;
                    }

                    switch (await this.RefreshAccessTokenAsync(cancellationToken))
                    {
                        case TokenRefreshOutcome.Refreshed:
                            return await this.SendAsync<TRequest, TResponse>(request, apiProvider, errors, false, cancellationToken);
                        case TokenRefreshOutcome.Rejected:
                            await authorisationService.SignOutAsync();
                            errors.Invoke(new ValidationProblemDetails()
                            {
                                Title = "Signed out.",
                                Status = (int)HttpStatusCode.Unauthorized,
                                Detail = "Your session has ended. Please sign in again.",
                                Errors = new Dictionary<string, string[]>()
                            });
                            return default;
                        default:
                            // The API could not be asked whether the session is still good, so it
                            // keeps the benefit of the doubt — the next attempt may well succeed.
                            errors.Invoke(new ValidationProblemDetails()
                            {
                                Title = "Cannot reach the API.",
                                Status = (int)HttpStatusCode.ServiceUnavailable,
                                Detail = "The API is not reachable. Please ensure it is running.",
                                Errors = new Dictionary<string, string[]>()
                            });
                            return default;
                    }
                case HttpStatusCode.UnprocessableContent:
                    errors.Invoke(JsonSerializer.Deserialize<ValidationProblemDetails>(_Content, JsonOptions.DefaultOptions)!);
                    return default;
                default:
                    errors.Invoke(new ValidationProblemDetails()
                    {
                        Title = "An error occurred.",
                        Status = (int)_HttpResponse.StatusCode,
                        Detail = $"The server returned an unexpected response ({(int)_HttpResponse.StatusCode}).",
                        Errors = new Dictionary<string, string[]>()
                    });
                    return default;
            }
        }
        catch (HttpRequestException)
        {
            errors.Invoke(new ValidationProblemDetails()
            {
                Title = "Cannot reach the API.",
                Status = (int)HttpStatusCode.ServiceUnavailable,
                Detail = "The API is not reachable. Please ensure it is running.",
                Errors = new Dictionary<string, string[]>()
            });
        }
        catch (TaskCanceledException)
        {
            errors.Invoke(new ValidationProblemDetails()
            {
                Title = "Request timed out.",
                Status = (int)HttpStatusCode.RequestTimeout,
                Detail = "The request timed out. Please try again.",
                Errors = new Dictionary<string, string[]>()
            });
        }
        catch (Exception _Exception)
        {
            errors.Invoke(new ValidationProblemDetails()
            {
                Title = "An unexpected error occurred.",
                Detail = _Exception.Message,
                Errors = new Dictionary<string, string[]>()
            });
        }

        return default;
    }

    private string GetClientCredentials()
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{configurationManager.GetValue<string>("OAuth:AccessToken:AccessToken")!}:{configurationManager.GetValue<string>("OAuth:AccessToken:ClientSecret")!}"));

    /// <summary>
    /// Spends the stored refresh token for a new access token, letting only one caller at a time
    /// past the gate. Callers that queued behind a refresh that has already succeeded take the
    /// token it produced instead of spending a refresh token that no longer exists.
    /// </summary>
    public async Task<TokenRefreshOutcome> RefreshAccessTokenAsync(CancellationToken cancellationToken)
    {
        var _SpentAccessToken = (await authorisationService.GetTokenAsync())?.AccessToken;

        try
        {
            await this.m_RefreshGate.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return TokenRefreshOutcome.Unavailable;
        }

        try
        {
            var _CurrentToken = await authorisationService.GetTokenAsync();

            if (_CurrentToken == null || string.IsNullOrEmpty(_CurrentToken.RefreshToken))
                return TokenRefreshOutcome.Rejected;

            if (_CurrentToken.AccessToken != _SpentAccessToken)
                return TokenRefreshOutcome.Refreshed;

            return await this.RequestRefreshedTokenAsync(_CurrentToken.RefreshToken, cancellationToken);
        }
        finally
        {
            _ = this.m_RefreshGate.Release();
        }
    }

    private async Task<TokenRefreshOutcome> RequestRefreshedTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var _HttpRequestMessage = BuildMessage(
                new CreateRefreshGrantWebAppRequest()
                {
                    ClientID = configurationManager.GetValue<long>("OAuth:AccessToken:ClientID")!,
                    ClientSecret = configurationManager.GetValue<string>("OAuth:AccessToken:ClientSecret")!,
                    GrantType = AuthorisationValues.RefreshGrantType,
                    RefreshToken = refreshToken
                },
                ApiProvider.GetRefreshToken());

            _HttpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", this.GetClientCredentials());

            var _HttpResponse = await httpClient.SendAsync(_HttpRequestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken);

            // Only the token endpoint turning the refresh token down is proof the session is over.
            // Anything else — a 500, a proxy error, an API that is simply not up yet — is not.
            if (!_HttpResponse.IsSuccessStatusCode)
                return _HttpResponse.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest
                    ? TokenRefreshOutcome.Rejected
                    : TokenRefreshOutcome.Unavailable;

            var _Response = JsonSerializer.Deserialize<CreateRefreshGrantWebAppResponse>(
                await _HttpResponse.Content.ReadAsStringAsync(cancellationToken),
                JsonOptions.DefaultOptions);

            if (_Response == null
                || string.IsNullOrEmpty(_Response.AccessToken)
                || string.IsNullOrEmpty(_Response.RefreshToken))
                return TokenRefreshOutcome.Unavailable;

            return await authorisationService.TryRefreshAsync(_Response, cancellationToken)
                ? TokenRefreshOutcome.Refreshed
                : TokenRefreshOutcome.Unavailable;
        }
        catch (HttpRequestException)
        {
            return TokenRefreshOutcome.Unavailable;
        }
        catch (OperationCanceledException)
        {
            // Covers TaskCanceledException, which HttpClient raises for both a timeout and a
            // cancelled token. Neither says anything about whether the session is still good.
            return TokenRefreshOutcome.Unavailable;
        }
        catch (JsonException)
        {
            return TokenRefreshOutcome.Unavailable;
        }
    }

    async Task<bool> IHomeHttpClient.TryLoginAsync(
        CreatePasswordGrantWebAppRequest request,
        Action<ValidationProblemDetails> problemDetails,
        CancellationToken cancellationToken)
    {
        if (loginThrottle.GetLockout(request.Username) is { } _Lockout)
        {
            problemDetails.Invoke(new ValidationProblemDetails()
            {
                Title = "Too many attempts.",
                Status = (int)HttpStatusCode.TooManyRequests,
                Detail = $"Wait about {Math.Max(1, (int)Math.Ceiling(_Lockout.TotalMinutes))} minute(s) and try again.",
                Errors = new Dictionary<string, string[]>()
            });

            return false;
        }

        var _Request = new CreatePasswordGrantWebAppRequest()
        {
            ClientID = configurationManager.GetValue<long>("OAuth:AccessToken:ClientID")!,
            ClientSecret = configurationManager.GetValue<string>("OAuth:AccessToken:ClientSecret")!,
            GrantType = configurationManager.GetValue<string>("OAuth:AccessToken:GrantType")!,
            Scope = configurationManager.GetValue<string>("OAuth:AccessToken:Scope")!,
            Username = request.Username,
            Password = request.Password
        };

        var _Response = await this.SendRequestAsync<CreatePasswordGrantWebAppRequest, CreatePasswordGrantWebAppResponse>(
            _Request,
            ApiProvider.GetOAuthToken(),
            problemDetails,
            cancellationToken);

        if (_Response == null)
        {
            loginThrottle.RecordFailure(request.Username);
            return false;
        }

        loginThrottle.RecordSuccess(request.Username);

        _ = await authorisationService.TrySignInAsync(_Request, _Response, cancellationToken);
        return true;
    }

    #endregion Methods

}
