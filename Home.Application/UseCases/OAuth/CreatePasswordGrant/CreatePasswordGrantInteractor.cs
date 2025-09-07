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

        var _ClientApplication = _PersistenceContext.GetEntities<ClientApplication>()
            .Single(ca => ca.ClientApplicationID == inputPort.ClientID);

        // Work out scopes

        var _User = _PersistenceContext.GetEntities<User>()
            .SingleOrDefault(u => u.Email == inputPort.Username);

        if (!await _PasswordService.VerifyPasswordAsync(_User, inputPort.Password, cancellationToken))
        {
            await outputPort.PresentNotAuthorisedAsync(OAuthValues.InvalidUsernameOrPassword, cancellationToken);
            return;
        }

        var _AccessToken = _TokenFactory.GetOAuthToken();
        var _RefreshToken = _TokenFactory.GetOAuthToken();

        var _AuthenticationMetadata = new Domain.Entities.AuthenticationMetadata()
        {
            AccessToken = _AccessToken,
            ClientApplication = _ClientApplication,
            DateSetUTC = DateTime.UtcNow,
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
