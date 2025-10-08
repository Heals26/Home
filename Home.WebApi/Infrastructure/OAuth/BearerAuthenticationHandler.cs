using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.UseCases.ApiAuditing;
using Home.WebApi.Infrastructure.Values;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Home.WebApi.Infrastructure.OAuth;

public class BearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{

    #region Fields

    private readonly IPersistenceContext m_PersistenceContext;

    #endregion Fields

    #region Constructors

    public BearerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IPersistenceContext persistenceContext)
        : base(options, logger, encoder)
        => this.m_PersistenceContext = persistenceContext;

    #endregion Constructors

    #region Methods

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!this.Request.Headers.TryGetValue(FrameworkValues.Authorisation, out var _AuthorisationHeaderValue))
        {
            var _ErrorMessage = "Cannot read Authorization header";
            this.SetApiAuditEntry(null, $"{nameof(BearerAuthenticationHandler)} {this.Request.RouteValues["action"]}", _ErrorMessage);
            return AuthenticateResult.Fail(_ErrorMessage);
        }

        try
        {
            var _AuthorizationToken = _AuthorisationHeaderValue.ToString().Split(' ');
            if (_AuthorizationToken.Length != 2)
                return AuthenticateResult.Fail("Invalid Token");

            var _AccessToken = _AuthorizationToken.Last();

            var _AuthenticationMetadata = this.m_PersistenceContext.GetEntities<Domain.Entities.UserAuthentication>()
                .Where(ca => ca.AccessToken == _AccessToken)
                .Select(am => new
                {
                    AuthenticationMetadata = am,
                    am.ClientApplication,
                    am.User
                })
                .SingleOrDefault()
                ?.AuthenticationMetadata;

            if (_AuthenticationMetadata == null || !this.TryValidateAuthorisationString(_AuthorisationHeaderValue, out _AccessToken))
            {
                this.SetApiAuditEntry(null, nameof(TryValidateAuthorisationString), "Invalid Token");
                return AuthenticateResult.Fail("Invalid Token");
            }

            if (_AuthenticationMetadata.DateSetUTC.AddYears(1) < DateTime.UtcNow)
            {
                this.SetApiAuditEntry(null, nameof(TryValidateAuthorisationString), "Access token is invalid or has expired.");
                return AuthenticateResult.Fail("Access token is invalid or has expired.");
            }

            var _OAuthMetadata = new AuthenticationMetadata()
            {
                ClientApplicationID = _AuthenticationMetadata.ClientApplication.ClientApplicationID,
                ClientName = _AuthenticationMetadata.ClientApplication.Name,
                Scopes = _AuthenticationMetadata.Scopes,
                UserID = _AuthenticationMetadata.User.UserID
            };

            var _ClaimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new List<Claim>([
                        new(nameof(AuthenticationMetadata.UserID), _OAuthMetadata.UserID.ToString(), ClaimValueTypes.Integer64),
                        new(nameof(AuthenticationMetadata.ClientApplicationID), _OAuthMetadata.ClientApplicationID.ToString(), ClaimValueTypes.Integer64),
                        new(nameof(AuthenticationMetadata.Scopes), _OAuthMetadata.Scopes, ClaimValueTypes.String),
                        new(nameof(AuthenticationMetadata.ClientName), _OAuthMetadata.ClientName.ToString(), ClaimValueTypes.String)
                    ])));

            var _Ticket = new AuthenticationTicket(_ClaimsPrincipal, this.Scheme.Name);
            this.SetApiAuditEntry(_OAuthMetadata, $"{nameof(BearerAuthenticationHandler)} {this.Request.RouteValues["action"]}", null);

            return AuthenticateResult.Success(_Ticket);
        }
        catch (Exception _Exception)
        {
            this.SetApiAuditEntry(null, nameof(HandleAuthenticateAsync), _Exception.Message);
            return AuthenticateResult.Fail(_Exception.Message);
        }
    }

    private bool TryValidateAuthorisationString(string authorisationHeader, out string accessToken)
    {
        accessToken = string.Empty;

        if (!authorisationHeader.StartsWith(FrameworkValues.Bearer))
            return false;

        var _AuthorisationValues = authorisationHeader.Split(' ', 2);

        if (_AuthorisationValues.Length != 2)
            return false;

        accessToken = _AuthorisationValues.Last();

        return true;
    }

    private void SetApiAuditEntry(AuthenticationMetadata authenticationMetadata, string actionName, string errors)
    {
        var _ApiAuditEntry = this.Context.RequestServices.GetRequiredService<CreateApiAuditEntry>();

        _ApiAuditEntry.AuthenticationData.ClientApplicationID = _ApiAuditEntry.AuthenticationData.ClientApplicationID ?? authenticationMetadata?.ClientApplicationID;
        _ApiAuditEntry.AuthenticationData.UserID = _ApiAuditEntry.AuthenticationData.UserID ?? authenticationMetadata?.UserID;

        _ApiAuditEntry.ActionData.ActionName = actionName;
        _ApiAuditEntry.ActionData.Details = errors;
    }

    #endregion Methods

}
