using Home.WebUI.Infrastructure.ApiProviders.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebUI.Infrastructure.Services.HttpClients;

public interface IHomeHttpClient
{

    #region Methods

    Task<TResponse?> SendRequestAsync<TRequest, TResponse>(
        TRequest request,
        ApiProviderHelper apiProvider,
        Action<ValidationProblemDetails> errors,
        CancellationToken cancellationToken);

    #endregion Methods

}
