using Home.WebUI.Infrastructure.ApiProviders.Helpers;
using Home.WebUI.Infrastructure.Services.HttpClients;
using Home.WebUI.Infrastructure.Services.Security;
using Home.WebUI.Infrastructure.Values;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Home.WebUI.Infrastructure.HttpClients;

/// <summary>
/// Talks to the API on behalf of whoever this circuit belongs to. It no longer knows anything about
/// signing in: the cookie does that, <see cref="IHouseholdSession"/> turns it into an access token,
/// and a 401 here is a token to replace rather than a session to end.
/// </summary>
public class HomeHttpClient(
    HttpClient httpClient,
    IHouseholdSession householdSession)
    : IHomeHttpClient
{

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
            var _AccessToken = await householdSession.GetAccessTokenAsync(cancellationToken);

            // Build a fresh HttpRequestMessage per attempt — they cannot be reused across calls
            var _HttpRequestMessage = BuildMessage(request, apiProvider);

            if (!string.IsNullOrEmpty(_AccessToken))
            {
                _HttpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _AccessToken);
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
                    if (!allowRefresh)
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

                    switch (await householdSession.RefreshAsync(_AccessToken, cancellationToken))
                    {
                        case TokenRefreshOutcome.Refreshed:
                            return await this.SendAsync<TRequest, TResponse>(request, apiProvider, errors, false, cancellationToken);
                        case TokenRefreshOutcome.Rejected:
                            // Nothing is signed out from here. The cookie is what holds the session
                            // and only the sign-out endpoint may clear it; a refused refresh means
                            // this page needs reloading, which the reconnect handling takes care of.
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

    #endregion Methods

}
