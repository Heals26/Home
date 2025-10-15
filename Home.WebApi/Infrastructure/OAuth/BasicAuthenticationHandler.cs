using Home.Application.Infrastructure.Values;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.UseCases.ApiAuditing;
using Home.WebApi.Infrastructure.Values;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Home.WebApi.Infrastructure.OAuth;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{

    #region Fields

    private readonly IPersistenceContext m_PersistenceContext;

    #endregion Fields

    #region Constructors

    public BasicAuthenticationHandler(
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
            this.SetApiAuditEntry(null, $"{nameof(BasicAuthenticationHandler)} {this.Request.RouteValues["action"]}", _ErrorMessage);
            return AuthenticateResult.Fail(_ErrorMessage);
        }

        try
        {
            (string clientSecret, string accessToken) _AccessToken = (string.Empty, string.Empty);
            if (!this.TryValidateAuthorisationString(_AuthorisationHeaderValue, out _AccessToken))
            {
                this.SetApiAuditEntry(null, nameof(TryValidateAuthorisationString), "Invalid Token");
                return AuthenticateResult.Fail("Invalid Token");
            }

            var _ClientApplication = this.m_PersistenceContext.GetEntities<Domain.Entities.ClientApplication>()
                .SingleOrDefault(ca => ca.AccessToken == _AccessToken.clientSecret && ca.Secret == _AccessToken.accessToken);

            if (_ClientApplication == null)
            {
                this.SetApiAuditEntry(null, nameof(TryValidateAuthorisationString), OAuthValues.InvalidClient.Name);
                return AuthenticateResult.Fail(OAuthValues.InvalidClient.Name);
            }

            var _AuthenticationMetadata = new AuthenticationMetadata()
            {
                ClientApplicationID = _ClientApplication.ClientApplicationID,
                ClientName = _ClientApplication.Name,
                Scopes = string.Empty
            };

            ClaimsPrincipal _ClaimsPrincipal = new(
                new ClaimsIdentity(
                    new List<Claim>([

                        new(nameof(AuthenticationMetadata.ClientApplicationID), _AuthenticationMetadata.ClientApplicationID.ToString(), ClaimValueTypes.Integer64),
                        new(nameof(AuthenticationMetadata.Scopes), _AuthenticationMetadata.Scopes, ClaimValueTypes.String),
                        new(nameof(AuthenticationMetadata.ClientName), _AuthenticationMetadata.ClientName.ToString(), ClaimValueTypes.String)
                    ])));

            AuthenticationTicket _Ticket = new(_ClaimsPrincipal, this.Scheme.Name);
            this.SetApiAuditEntry(_AuthenticationMetadata, $"{nameof(BasicAuthenticationHandler)} {this.Request.RouteValues["action"]}", null);

            return AuthenticateResult.Success(_Ticket);
        }
        catch (Exception _Exception)
        {
            this.SetApiAuditEntry(null, nameof(HandleAuthenticateAsync), _Exception.Message);
            return AuthenticateResult.Fail(_Exception.Message);
        }
    }

    private bool TryValidateAuthorisationString(string authorisationHeader, out (string accessToken, string clientSecret) token)
    {
        token = (string.Empty, string.Empty);

        if (!authorisationHeader.StartsWith(FrameworkValues.Basic))
            return false;

        var _AuthorisationValues = Encoding.UTF8.GetString(Convert.FromBase64String(authorisationHeader.Split(' ').Last())).Split(':', 2);

        if (_AuthorisationValues.Length != 2)
            return false;

        token = (_AuthorisationValues.First(), _AuthorisationValues.Last());

        return true;
    }

    private void SetApiAuditEntry(AuthenticationMetadata authenticationMetadata, string actionName, string errors)
    {
        var _ApiAuditEntry = this.Context.RequestServices.GetRequiredService<CreateApiAuditEntry>();

        _ApiAuditEntry.AuthenticationData.ClientApplicationID = authenticationMetadata?.ClientApplicationID;
        _ApiAuditEntry.AuthenticationData.UserID = authenticationMetadata?.UserID;

        _ApiAuditEntry.ActionData.ActionName = actionName;
        _ApiAuditEntry.ActionData.Details = errors;
    }

    #endregion Methods

}
