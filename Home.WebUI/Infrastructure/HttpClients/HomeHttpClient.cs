using Home.WebUI.Infrastructure.ApiProviders.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebUI.Infrastructure.HttpClients;

public class HomeHttpClient(HttpClient httpClient)
{

    #region Methods

    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, ApiProviderHelper apiProvider, Action<ProblemDetails> errors, CancellationToken cancellationToken)
    {

    }

    private async Task<TResponse> SendRequestAsync<TResponse>(HttpRequestMessage httpMessage, Action<ProblemDetails> errors, CancellationToken cancellationToken)
    {
        try
        {
            var _HttpResponse = await httpClient.SendAsync(httpMessage, HttpCompletionOption.ResponseContentRead, cancellationToken);
            var _Content = await _HttpResponse.Content.ReadAsStringAsync();

        }
        catch (HttpRequestException)
        {

        }
        catch (TaskCanceledException)
        {

        }
    }

    #endregion Methods

}
