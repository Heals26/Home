using AutoMapper;
using Home.Application.Services.Persistence;
using Home.Application.UseCases.ApiAuditing;
using Home.Domain.Entities;
using System.Text;
using System.Text.Json;

namespace Home.WebApi.Middleware;

public class ApiAuditing(RequestDelegate next, TimeProvider timeProvider)
{

    #region Methods

    public async Task InvokeAsync(HttpContext context)
    {
        var _AuditEntry = context.RequestServices.GetRequiredService<CreateApiAuditEntry>();
        var _Faulted = false;

        // Downstream still has to read the body after we have, so buffering has to be on before
        // anything touches the stream.
        context.Request.EnableBuffering();

        await ApiAuditing.SetAuditEntryRequestDataAsync(_AuditEntry.RequestData, context, timeProvider.GetUtcNow().UtcDateTime);

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // An escaping exception is exactly what the trail exists to explain, so it is recorded
            // on the way past rather than lost.
            _Faulted = true;
            _AuditEntry.ActionData.Details = $"{ex.GetType().FullName}: {ex.Message}";

            throw;
        }
        finally
        {
            await ApiAuditing.WriteAuditEntryAsync(_AuditEntry, context, timeProvider.GetUtcNow().UtcDateTime, _Faulted);
        }
    }

    private static string GetRequestAbsoluteUriString(HttpContext context)
    {
        var _UriBuilder = new UriBuilder()
        {
            Scheme = context.Request.Scheme,
            Host = context.Request.Host.Host,
            Port = context.Request.Host.Port.GetValueOrDefault(-1),
            Path = context.Request.Path,
            Query = context.Request.QueryString.ToString()
        };

        return _UriBuilder.Uri.AbsoluteUri;
    }

    // Credential-bearing endpoints. Their bodies carry passwords and client secrets and must never
    // reach the audit table, whatever else changes here.
    private static bool IsCredentialBearingPath(PathString path)
        => path.StartsWithSegments("/api/OAuth", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/api/Households/register", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/api/Users", StringComparison.OrdinalIgnoreCase);

    private static bool IsHeartbeatRequest(CreateApiAuditEntry.ApiAuditEntryRequestData requestData)
    {
        const string EMPTY_URI_PATH_AND_QUERY = "/";
        const string LOOPBACK_ADDRESS_IPV4 = "127.0.0.1";
        const string LOOPBACK_ADDRESS_IPV6 = "::1";

        if (!Uri.TryCreate(requestData.RequestUri, UriKind.Absolute, out var _RequestUri))
            return false;

        var _IPAddress = requestData.RemoteIPAddress;

        return (_IPAddress == LOOPBACK_ADDRESS_IPV4 || _IPAddress == LOOPBACK_ADDRESS_IPV6) && (_RequestUri.PathAndQuery == EMPTY_URI_PATH_AND_QUERY);
    }

    private static async Task<string> ReadRequestBodyAsync(HttpContext context)
    {
        const int MAXIMUM_BODY_CHARACTERS = 4000;

        if (ApiAuditing.IsCredentialBearingPath(context.Request.Path))
            return string.Empty;

        if (!context.Request.Body.CanSeek || context.Request.ContentLength is null or 0)
            return string.Empty;

        context.Request.Body.Position = 0;

        using var _Reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var _Buffer = new char[MAXIMUM_BODY_CHARACTERS];
        var _CharactersRead = await _Reader.ReadBlockAsync(_Buffer, 0, MAXIMUM_BODY_CHARACTERS);

        // Model binding has not run yet, so the stream has to be handed back at the start.
        context.Request.Body.Position = 0;

        return new string(_Buffer, 0, _CharactersRead);
    }

    private static async Task SaveAuditEntryAsync(CreateApiAuditEntry createAuditEntryCommand, IAuditPersistenceContext dbContext, IMapper mapper, ILogger logger)
    {
        try
        {
            var _AuditEntry = mapper.Map<ApiAuditEntry>(createAuditEntryCommand);
            dbContext.Add(_AuditEntry);
            _ = await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Serialise the ApiAuditEntry and log it.
            var _ApiAuditEntryJson = JsonSerializer.Serialize(createAuditEntryCommand);
            logger.LogError(ex, _ApiAuditEntryJson);
        }
    }

    private static async Task SetAuditEntryRequestDataAsync(
        CreateApiAuditEntry.ApiAuditEntryRequestData requestData,
        HttpContext context,
        DateTime nowUTC)
    {
        requestData.RequestReceivedOnUTC = nowUTC;

        // Capturing the request must never be the reason a request fails.
        try
        {
            requestData.RemoteIPAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            requestData.RequestBody = await ApiAuditing.ReadRequestBodyAsync(context);
            requestData.RequestUri = ApiAuditing.GetRequestAbsoluteUriString(context);
            requestData.UserAgent = TruncateUserAgent(context.Request.Headers["User-Agent"].ToString());

            var _Version = context.GetRequestedApiVersion()?.ToString();
            requestData.Version = string.IsNullOrEmpty(_Version) ? "Unversioned" : _Version;
        }
        catch (Exception ex)
        {
            context.RequestServices.GetService<ILogger<ApiAuditing>>()?
                .LogError(ex, "Failed to capture request data for the API audit trail.");
        }

        static string TruncateUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return null;
            else if (userAgent.Length > 500)
                return userAgent.Substring(0, 499);
            else
                return userAgent;
        }
    }

    private static void SetAuditEntryResponseData(
        CreateApiAuditEntry.ApiAuditEntryResponseData responseData,
        HttpContext context,
        DateTime nowUTC,
        bool faulted)
    {
        // An exception unwinds before the response is written, so the context still reports 200
        // even though the caller will be handed a 500.
        responseData.HttpResponseStatusCode = faulted
            ? (short)StatusCodes.Status500InternalServerError
            : (short)context.Response.StatusCode;

        responseData.ResponseSentOnUTC = nowUTC;
    }

    private static async Task WriteAuditEntryAsync(
        CreateApiAuditEntry auditEntry,
        HttpContext context,
        DateTime nowUTC,
        bool faulted)
    {
        try
        {
            ApiAuditing.SetAuditEntryResponseData(auditEntry.ResponseData, context, nowUTC, faulted);

            // Heartbeats and the SignalR hub would drown the trail in noise.
            if (ApiAuditing.IsHeartbeatRequest(auditEntry.RequestData)
                || !context.Request.Path.HasValue
                || context.Request.Path.StartsWithSegments("/hubs", StringComparison.OrdinalIgnoreCase)
                || context.Request.Headers.ContainsKey("blazor-hub"))
                return;

            var _DbContext = context.RequestServices.GetRequiredService<IAuditPersistenceContext>();
            var _Mapper = context.RequestServices.GetRequiredService<IMapper>();
            var _Logger = context.RequestServices.GetRequiredService<ILogger<ApiAuditing>>();

            await ApiAuditing.SaveAuditEntryAsync(auditEntry, _DbContext, _Mapper, _Logger);
        }
        catch (Exception ex)
        {
            // Nothing about auditing may surface to the caller, including a failure to audit.
            context.RequestServices.GetService<ILogger<ApiAuditing>>()?
                .LogError(ex, "Failed to write an API audit entry.");
        }
    }

    #endregion Methods

}
