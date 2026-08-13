// Home.WebApi has nullable disabled project-wide; this file reasons about absent tokens, so it
// opts in the same way LifxLightService does.
#nullable enable

using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using System.Net.Http.Headers;

namespace Home.WebApi.Infrastructure.Lights;

/// <summary>
/// Attaches the LIFX bearer token to every outgoing provider request. The authenticated
/// household's stored token wins; the <c>lifxApiToken</c> user secret is a developer fallback.
/// Background work (the schedule runner) has no request context, so it uses the single stored
/// token if exactly one household has one — a multi-household deployment will need the runner
/// to carry the household through explicitly, which is noted in DECISIONS.md.
/// </summary>
internal class LifxAuthenticationHandler(
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor,
    IServiceScopeFactory serviceScopeFactory) : DelegatingHandler
{

    #region Methods

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var _Token = this.ResolveToken();

        if (!string.IsNullOrWhiteSpace(_Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _Token);

        return await base.SendAsync(request, cancellationToken);
    }

    private string? ResolveToken()
    {
        // Inside an authenticated request the caller's household decides. RequestServices is
        // used because this handler lives in HttpClientFactory's own scope, not the request's.
        var _RequestServices = httpContextAccessor.HttpContext?.RequestServices;

        if (_RequestServices != null)
        {
            var _Household = _RequestServices.GetRequiredService<IAuthorisationService>().GetHousehold();

            if (!string.IsNullOrWhiteSpace(_Household?.LifxApiToken))
                return _Household.LifxApiToken;
        }
        else
        {
            using var _Scope = serviceScopeFactory.CreateScope();

            var _Tokens = _Scope.ServiceProvider.GetRequiredService<IPersistenceContext>()
                .GetEntities<Household>()
                .Where(h => h.LifxApiToken != null && h.LifxApiToken != string.Empty)
                .Select(h => h.LifxApiToken)
                .Take(2)
                .ToList();

            if (_Tokens.Count == 1)
                return _Tokens[0];
        }

        return configuration["lifxApiToken"];
    }

    #endregion Methods

}
