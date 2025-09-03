using AutoMapper;
using Home.Application.Services.Persistence;
using Home.Application.UseCases.ApiAuditing;
using Home.Domain.Entities;
using System.Text.Json;

namespace Home.WebApi.Middleware;

public class ApiAuditing(RequestDelegate next)
{

    #region Methods

    public async Task InvokeAsync(HttpContext context)
    {
        var _AuditEntry = context.RequestServices.GetRequiredService<CreateApiAuditEntry>();

        ApiAuditing.SetAuditEntryRequestData(_AuditEntry.RequestData, context);

        // --------------------------------------------------------------------------------
        // Invoke next piece of Middleware in the request pipeline.

        await next(context);

        // --------------------------------------------------------------------------------
        // On Response Out

        // Set the Response Data.
        ApiAuditing.SetAuditEntryResponseData(_AuditEntry.ResponseData, context);

        // Persist the Audit Entry.
        // Do not pollute the Audit Entries with heartbeat requests.
        if (!IsHeartbeatRequest(_AuditEntry.RequestData) &&
            context.Request.Path.HasValue &&
            !context.Request.Path.Value.Contains("hangfire") &&
            (!context.Request.Headers.TryGetValue("blazor-hub", out var _)))
        {
            var _DbContext = context.RequestServices.GetRequiredService<IAuditPersistenceContext>();
            var _Mapper = context.RequestServices.GetRequiredService<IMapper>();
            var _Logger = context.RequestServices.GetRequiredService<ILogger<ApiAuditing>>();

            await SaveAuditEntryAsync(_AuditEntry, _DbContext, _Mapper, _Logger);
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

    private static bool IsHeartbeatRequest(CreateApiAuditEntry.ApiAuditEntryRequestData requestData)
    {
        const string EMPTY_URI_PATH_AND_QUERY = "/";
        const string LOOPBACK_ADDRESS_IPV4 = "127.0.0.1";
        const string LOOPBACK_ADDRESS_IPV6 = "::1";

        var _RequestUri = new Uri(requestData.RequestUri);
        var _IPAddress = requestData.RemoteIPAddress;

        return (_IPAddress == LOOPBACK_ADDRESS_IPV4 || _IPAddress == LOOPBACK_ADDRESS_IPV6) && (_RequestUri.PathAndQuery == EMPTY_URI_PATH_AND_QUERY);
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

    private static void SetAuditEntryRequestData(CreateApiAuditEntry.ApiAuditEntryRequestData requestData, HttpContext context)
    {
        requestData.RemoteIPAddress = context.Connection.RemoteIpAddress.ToString();
        requestData.RequestReceivedOnUTC = DateTime.UtcNow;
        requestData.RequestUri = ApiAuditing.GetRequestAbsoluteUriString(context);
        requestData.UserAgent = TruncateUserAgent(context.Request.Headers["User-Agent"].ToString());
        var _Version = context.GetRequestedApiVersion()?.ToString();
        requestData.Version = string.IsNullOrEmpty(_Version) ? "Unversioned" : _Version;

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

    private static void SetAuditEntryResponseData(CreateApiAuditEntry.ApiAuditEntryResponseData responseData, HttpContext context)
    {
        responseData.HttpResponseStatusCode = (short)context.Response.StatusCode;
        responseData.ResponseSentOnUTC = DateTime.UtcNow;
    }

    #endregion Methods

}
