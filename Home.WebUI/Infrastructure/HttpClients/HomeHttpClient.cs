using Home.WebUI.Infrastructure.ApiProviders.Helpers;
using Home.WebUI.Infrastructure.Services.HttpClients;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Home.WebUI.Infrastructure.HttpClients;

public class HomeHttpClient(HttpClient httpClient)
    : IHomeHttpClient
{

    #region Methods

    public async Task<TResponse?> SendRequestAsync<TRequest, TResponse>(
        TRequest request,
        ApiProviderHelper apiProvider,
        Action<ProblemDetails> errors,
        CancellationToken cancellationToken)
    {
        var _HttpRequestMessage = new HttpRequestMessage(apiProvider.HttpMethod, apiProvider.Uri)
        {
            Content = apiProvider.RouteType.GetHttpRequestMessage(request)
        };

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

    private async Task<TResponse?> SendAsync<TResponse>(HttpRequestMessage httpMessage, Action<ValidationProblemDetails> errors, CancellationToken cancellationToken)
    {
        try
        {
            var _HttpResponse = await httpClient.SendAsync(httpMessage, HttpCompletionOption.ResponseContentRead, cancellationToken);
            var _Content = await _HttpResponse.Content.ReadAsStringAsync();

            if (_HttpResponse.IsSuccessStatusCode)
            {
                return typeof(TResponse) == typeof(bool)
                    ? (TResponse)(object)true
                    : JsonSerializer.Deserialize<TResponse>(_Content);
            }



            switch (_HttpResponse.StatusCode)
            {
                case HttpStatusCode.NotFound:
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Unauthorized:
                    errors.Invoke(this.ConvertProblemDetailsToValidationProblemDetails(JsonSerializer.Deserialize<ProblemDetails>(_Content)!));
                    return default;
                case HttpStatusCode.UnprocessableContent:
                    errors.Invoke(JsonSerializer.Deserialize<ValidationProblemDetails>(_Content)!);
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

    #endregion Methods

}
