using Home.WebUI.Infrastructure.Services.Security;
using Home.WebUI.Infrastructure.Values;

namespace Home.WebUI.Endpoints;

/// <summary>
/// The one place the browser fetches an image rather than the circuit fetching data: an img tag
/// can only send the sign-in cookie, so this endpoint turns the cookie into a bearer token and
/// streams the photo through from the API — which keeps the API's household check in charge of
/// who sees what.
/// </summary>
public static class RecipeImageEndpoints
{

    #region Methods

    public static void MapRecipeImageEndpoints(this IEndpointRouteBuilder endpoints)
        => _ = endpoints.MapGet("/recipe-images/{recipeID:long}", GetRecipeImageAsync)
            .RequireAuthorization();

    private static async Task<IResult> GetRecipeImageAsync(
        HttpContext httpContext,
        IHouseholdSession householdSession,
        IHttpClientFactory httpClientFactory,
        long recipeID,
        CancellationToken cancellationToken)
    {
        var _AccessToken = await householdSession.GetAccessTokenAsync(cancellationToken);

        if (string.IsNullOrEmpty(_AccessToken))
            return Results.Unauthorized();

        var _Client = httpClientFactory.CreateClient(HttpClientValues.ApiClientName);

        var _Message = new HttpRequestMessage(HttpMethod.Get, $"api/Recipes/{recipeID}/Image");
        _Message.Headers.Add("api-version", "1.0");
        _Message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _AccessToken);

        var _Response = await _Client.SendAsync(_Message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!_Response.IsSuccessStatusCode)
            return Results.NotFound();

        // The URL carries the photo's version, so a changed photo is a changed URL and this can
        // be cached as hard as the browser likes.
        httpContext.Response.Headers.CacheControl = "private, max-age=31536000, immutable";

        return Results.Stream(
            await _Response.Content.ReadAsStreamAsync(cancellationToken),
            _Response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream");
    }

    #endregion Methods

}
