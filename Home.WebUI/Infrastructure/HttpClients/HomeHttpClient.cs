using Home.WebUI.Infrastructure.ApiProviders.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Home.WebUI.Infrastructure.HttpClients;

public class HomeHttpClient(HttpClient httpClient)
{

    #region Methods

    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, ApiProviderHelper apiProvider, Action<ProblemDetails> errors, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task<TResponse> SendRequestAsync<TResponse>(HttpRequestMessage httpMessage, Action<ProblemDetails> errors, CancellationToken cancellationToken)
    {
        try
        {
            var _HttpResponse = await httpClient.SendAsync(httpMessage, HttpCompletionOption.ResponseContentRead, cancellationToken);
            var _Content = await _HttpResponse.Content.ReadAsStringAsync();

            if (_HttpResponse.IsSuccessStatusCode)
                return JsonSerializer.Deserialize<TResponse>(_Content);

            errors.Invoke(JsonSerializer.Deserialize<ProblemDetails>(_Content));
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
