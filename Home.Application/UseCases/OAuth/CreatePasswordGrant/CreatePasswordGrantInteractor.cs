using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.Values;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Users;

namespace Home.Application.UseCases.OAuth.CreatePasswordGrant;

internal class CreatePasswordGrantInteractor : IInteractor<CreatePasswordGrantInputPort, ICreatePasswordGrantOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreatePasswordGrantInputPort inputPort,
        ICreatePasswordGrantOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PasswordService = serviceFactory.GetService<IPasswordService>();
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _TokenFactory = serviceFactory.GetService<ITokenFactory>();

        if (inputPort.ClientID == default
            || inputPort.ClientSecret == default
            || inputPort.Username == default
            || inputPort.Password == default)
        {
            await outputPort.PresentNotAuthorisedAsync(OAuthValues.InvalidClient, cancellationToken);
            return;
        }

        if (inputPort.GrantType == default || inputPort.GrantType != OAuthValues.GrantTypePassword.Name)
        {
            await outputPort.PresentNotAuthorisedAsync(OAuthValues.InvalidGrant, cancellationToken);
            return;
        }

        // The secret is compared, not merely required to be present. Until 4 Sep 2026 this looked
        // the client up by ID alone, so any caller who could reach the endpoint could mint a token
        // with client_id=1 and no knowledge of the secret at all. CreateRefreshGrant has always
        // compared it, which is what says this was an oversight rather than a decision.
        var _ClientApplication = _PersistenceContext.GetEntities<ClientApplication>()
            .SingleOrDefault(ca => ca.ClientApplicationID == inputPort.ClientID
                && ca.Secret == inputPort.ClientSecret);

        if (_ClientApplication == null)
        {
            await outputPort.PresentNotAuthorisedAsync(OAuthValues.InvalidClient, cancellationToken);
            return;
        }

        // Work out scopes

        var _User = _PersistenceContext.GetEntities<User>()
            .SingleOrDefault(u => u.Email == inputPort.Username);

        if (_User == null || !await _PasswordService.VerifyPasswordAsync(_User, inputPort.Password, cancellationToken))
        {
            await outputPort.PresentNotAuthorisedAsync(OAuthValues.InvalidUsernameOrPassword, cancellationToken);
            return;
        }

        var _AccessToken = _TokenFactory.GetOAuthToken();
        var _RefreshToken = _TokenFactory.GetOAuthToken();
        var _Now = serviceFactory.GetService<TimeProvider>().GetUtcNow().UtcDateTime;

        var _AuthenticationMetadata = new UserAuthentication()
        {
            AccessToken = _AccessToken,
            ClientApplication = _ClientApplication,
            DateSetUTC = _Now,
            DeviceLabel = inputPort.DeviceLabel,
            ExpiresOnUTC = _Now.Add(SessionValues.RefreshTokenLifetime),
            LastUsedOnUTC = _Now,
            RefreshToken = _RefreshToken,
            Scopes = string.Join(",", [OAuthValues.WebAppScope.Name]),
            User = _User
        };

        _PersistenceContext.Add(_AuthenticationMetadata);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentAuthorisationGrantedAsync(_AuthenticationMetadata, cancellationToken);
    }

    #endregion Methods

}
