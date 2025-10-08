using Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;
using Home.WebUI.DataAccess.OAuth.CreateRefreshGrant;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.ApiProviders.Helpers;
using Home.WebUI.Infrastructure.Services.HttpClients;
using Home.WebUI.Infrastructure.Services.Security;
using Home.WebUI.Infrastructure.UriProvider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Home.WebUI.Infrastructure.HttpClients;

public class HomeHttpClient(
    IAuthorisationService authorisationService,
    ConfigurationManager configurationManager,
    HttpClient httpClient,
    HttpContextAccessor httpContextAccessor)
    : IHomeHttpClient
{

    #region Methods

    public async Task<TResponse?> SendRequestAsync<TRequest, TResponse>(
        TRequest request,
        ApiProviderHelper apiProvider,
        Action<ValidationProblemDetails> errors,
        CancellationToken cancellationToken)
    {
        var _HttpRequestMessage = new HttpRequestMessage(apiProvider.HttpMethod, apiProvider.Uri)
        {
            Content = apiProvider.RouteType.GetHttpRequestMessage(request)
        };

        _HttpRequestMessage.Headers.Add("api-version", apiProvider.Version);

        return await this.SendAsync<TResponse>(_HttpRequestMessage, errors, cancellationToken);
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
                {
                    { "Error", new string[] { problemDetails.Detail ?? "An error occurred." } }
                }
            };

    private async Task<TResponse?> SendAsync<TResponse>(
        HttpRequestMessage httpMessage,
        Action<ValidationProblemDetails> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            var _AccessToken = httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true
                ? await httpContextAccessor.HttpContext.GetTokenAsync("access_token")
                : null;

            var _IsPublicEndpoint = httpMessage.RequestUri?.AbsolutePath.Contains(AuthorisationUriProvider.GetLoginUri(), StringComparison.CurrentCultureIgnoreCase) ?? false;

            if (!_IsPublicEndpoint && !string.IsNullOrEmpty(_AccessToken))
            {
                httpMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _AccessToken);
            }
            else if (!_IsPublicEndpoint && string.IsNullOrEmpty(_AccessToken))
            {
                errors.Invoke(new()
                {
                    Title = "Unauthorized",
                    Status = (int)HttpStatusCode.Unauthorized,
                    Detail = "You are not authorized to access this resource.",
                    Instance = httpMessage.RequestUri?.ToString(),
                    Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    Errors = new Dictionary<string, string[]>()
                    {
                        { "Error", new string[] { "You are not authorized to access this resource." } }
                    }
                });
                return default;
            }

            var _HttpResponse = await httpClient.SendAsync(httpMessage, HttpCompletionOption.ResponseContentRead, cancellationToken);
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
                            Title = "Either the API isn't running or the endpoint doesn't exist.",
                            Status = (int)HttpStatusCode.NotFound,
                            Detail = "Validate that the API is running or you have the correct endpoint and API version.",
                            Instance = string.Empty,
                            Type = string.Empty,
                            Errors = new Dictionary<string, string[]>()
                            {
                                { "Error", new string[] { "Please ensure that everything is correctly running. If it isn't contact the system administrator." } }
                            }
                        });
                    else
                        errors.Invoke(this.ConvertProblemDetailsToValidationProblemDetails(JsonSerializer.Deserialize<ProblemDetails>(_Content, JsonOptions.DefaultOptions)!));
                    return default;
                case HttpStatusCode.BadRequest:
                    errors.Invoke(this.ConvertProblemDetailsToValidationProblemDetails(JsonSerializer.Deserialize<ProblemDetails>(_Content, JsonOptions.DefaultOptions)!));
                    return default;
                case HttpStatusCode.Unauthorized:
                    if (await this.TryRefreshTokenAsync(errors, cancellationToken))
                        return await this.SendAsync<TResponse>(httpMessage, errors, cancellationToken);

                    return default;
                case HttpStatusCode.UnprocessableContent:
                    errors.Invoke(JsonSerializer.Deserialize<ValidationProblemDetails>(_Content, JsonOptions.DefaultOptions)!);
                    return default;
                default:
                    errors.Invoke(new ValidationProblemDetails()
                    {
                        Title = "Error",
                        Status = (int)_HttpResponse.StatusCode,
                        Detail = "An error occurred.",
                        Instance = httpMessage.RequestUri?.ToString(),
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                        Errors = new Dictionary<string, string[]>()
                        {
                            { "Error", new string[] { "An error occurred." } }
                        }
                    });
                    return default;
            }
        }
        catch (HttpRequestException)
        {

        }
        catch (TaskCanceledException)
        {

        }

        throw new NotImplementedException();
    }

    private async Task<bool> TryRefreshTokenAsync(Action<ValidationProblemDetails> errors, CancellationToken cancellationToken)
    {
        var _HttpRequestMessage = new HttpRequestMessage(ApiProvider.GetOAuthToken().HttpMethod, ApiProvider.GetOAuthToken().Uri)
        {
            Content = ApiProvider.GetOAuthToken().RouteType.GetHttpRequestMessage(new CreateRefreshGrantWebAppRequest()
            {
                ClientID = configurationManager.GetValue<long>("OAuth:AccessToken:ClientID")!,
                ClientSecret = configurationManager.GetValue<string>("OAuth:AccessToken:ClientSecret")!,
                GrantType = configurationManager.GetValue<string>("OAuth:AccessToken:GrantType")!,
                RefreshToken = await httpContextAccessor.HttpContext?.GetTokenAsync("refresh_token")! ?? string.Empty
            })
        };

        var _HttpResponse = await httpClient.SendAsync(_HttpRequestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken);

        if (!_HttpResponse.IsSuccessStatusCode)
            return false;

        var _Response = JsonSerializer.Deserialize<CreateRefreshGrantWebAppResponse>(await _HttpResponse.Content.ReadAsStringAsync(cancellationToken), JsonOptions.DefaultOptions);
        if (_Response == null)
        {
            errors.Invoke(new ValidationProblemDetails()
            {
                Title = "Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "An error occurred while refreshing the token.",
                Instance = _HttpRequestMessage.RequestUri?.ToString(),
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Errors = new Dictionary<string, string[]>()
                    {
                        { "Error", new string[] { "An error occurred while refreshing the token." } }
                    }
            });

            return false;
        }

        _ = await authorisationService.TryRefreshAsync(_Response, cancellationToken);
        return true;
    }
    
    async Task<bool> IHomeHttpClient.TryLoginAsync(
        CreatePasswordGrantWebAppRequest request,
        Action<ValidationProblemDetails> problemDetails,
        CancellationToken cancellationToken)
    {
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

        if (_Response != null)
        {
            _ = await authorisationService.TrySignInAsync(_Request, _Response, cancellationToken);
            return true;
        }

        return false;
    }

    #endregion Methods

}
