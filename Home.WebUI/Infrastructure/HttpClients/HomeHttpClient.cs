using Home.WebUI.DataAccess.OAuth.CreatePasswordGrant;
using Home.WebUI.DataAccess.OAuth.CreateRefreshGrant;
using Home.WebUI.Infrastructure.ApiProviders;
using Home.WebUI.Infrastructure.ApiProviders.Helpers;
using Home.WebUI.Infrastructure.Services.HttpClients;
using Home.WebUI.Infrastructure.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Home.WebUI.Infrastructure.HttpClients;

public class HomeHttpClient(
    IAuthorisationService authorisationService,
    IConfiguration configurationManager,
    HttpClient httpClient)
    : IHomeHttpClient
{

    #region Methods

    public Task<TResponse?> SendRequestAsync<TRequest, TResponse>(
        TRequest request,
        ApiProviderHelper apiProvider,
        Action<ValidationProblemDetails> errors,
        CancellationToken cancellationToken)
        => this.SendAsync<TRequest, TResponse>(request, apiProvider, errors, cancellationToken);

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
        CancellationToken cancellationToken)
    {
        try
        {
            var _Token = await authorisationService.GetTokenAsync();
            var _IsAuthenticated = _Token != null && !string.IsNullOrEmpty(_Token.AccessToken);
            var _IsLoginEndpoint = apiProvider.Uri.Contains(ApiProvider.GetOAuthToken().Uri, StringComparison.OrdinalIgnoreCase);

            // Build a fresh HttpRequestMessage per attempt — they cannot be reused across calls
            var _HttpRequestMessage = BuildMessage(request, apiProvider);

            if (_IsAuthenticated)
            {
                _HttpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _Token!.AccessToken);
            }
            else if (_IsLoginEndpoint)
            {
                var _BasicCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                    $"{configurationManager.GetValue<string>("OAuth:AccessToken:AccessToken")!}:{configurationManager.GetValue<string>("OAuth:AccessToken:ClientSecret")!}"));

                _HttpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", _BasicCredentials);
            }
            else
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
                    // Access token was rejected by the API — attempt a reactive refresh and retry once
                    if (await this.TryRefreshTokenAsync(errors, cancellationToken))
                        return await this.SendAsync<TRequest, TResponse>(request, apiProvider, errors, cancellationToken);
                    await authorisationService.SignOutAsync();
                    return default;
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

    private async Task<bool> TryRefreshTokenAsync(Action<ValidationProblemDetails> errors, CancellationToken cancellationToken)
    {
        var _CurrentToken = await authorisationService.GetTokenAsync();

        var _HttpRequestMessage = BuildMessage(
            new CreateRefreshGrantWebAppRequest()
            {
                ClientID = configurationManager.GetValue<long>("OAuth:AccessToken:ClientID")!,
                ClientSecret = configurationManager.GetValue<string>("OAuth:AccessToken:ClientSecret")!,
                GrantType = configurationManager.GetValue<string>("OAuth:AccessToken:GrantType")!,
                RefreshToken = _CurrentToken?.RefreshToken ?? string.Empty
            },
            ApiProvider.GetOAuthToken());

        // The token endpoint authenticates the client application itself, not the user
        var _BasicCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{configurationManager.GetValue<string>("OAuth:AccessToken:AccessToken")!}:{configurationManager.GetValue<string>("OAuth:AccessToken:ClientSecret")!}"));

        _HttpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", _BasicCredentials);

        var _HttpResponse = await httpClient.SendAsync(_HttpRequestMessage, HttpCompletionOption.ResponseContentRead, cancellationToken);

        if (!_HttpResponse.IsSuccessStatusCode)
            return false;

        var _Response = JsonSerializer.Deserialize<CreateRefreshGrantWebAppResponse>(
            await _HttpResponse.Content.ReadAsStringAsync(cancellationToken),
            JsonOptions.DefaultOptions);

        if (_Response == null)
            return false;

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
