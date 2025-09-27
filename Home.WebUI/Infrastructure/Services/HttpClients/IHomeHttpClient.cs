using Home.WebUI.Infrastructure.ApiProviders.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebUI.Infrastructure.Services.HttpClients;

public interface IHomeHttpClient
{

    #region Methods

    Task<TResponse?> SendAsync<TRequest, TResponse>(
        TRequest request,
        ApiProviderHelper apiProvider,
        Action<ProblemDetails> errors,
        CancellationToken cancellationToken);

    #endregion Methods

}
