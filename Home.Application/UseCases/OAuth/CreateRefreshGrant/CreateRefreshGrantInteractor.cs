using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.Values;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Users;

namespace Home.Application.UseCases.OAuth.CreateRefreshGrant;

internal class CreateRefreshGrantInteractor : IInteractor<CreateRefreshGrantInputPort, ICreateRefreshGrantOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateRefreshGrantInputPort inputPort,
        ICreateRefreshGrantOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PasswordService = serviceFactory.GetService<IPasswordService>();
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _TokenFactory = serviceFactory.GetService<ITokenFactory>();

        if (inputPort.ClientID == default
            || inputPort.ClientSecret == default
            || inputPort.RefreshToken == default)
        {
            await outputPort.PresentNotAuthorisedAsync(OAuthValues.InvalidClient, cancellationToken);
            return;
        }

        if (inputPort.GrantType == default || inputPort.GrantType != OAuthValues.GrantTypeRefresh.Name)
        {
            await outputPort.PresentNotAuthorisedAsync(OAuthValues.InvalidGrant, cancellationToken);
            return;
        }

        var _ClientApplication = _PersistenceContext.GetEntities<ClientApplication>()
            .Single(ca => ca.ClientApplicationID == inputPort.ClientID && ca.Secret == inputPort.ClientSecret);

        var _ExistingToken = _PersistenceContext.GetEntities<Domain.Entities.UserAuthentication>()
            .Where(am => am.RefreshToken == inputPort.RefreshToken)
            .Select(am => new
            {
                AuthenticationMetadata = am,
                am.User
            })
            .SingleOrDefault()
            ?.AuthenticationMetadata;

        if (_ExistingToken == default)
        {
            await outputPort.PresentNotAuthorisedAsync(OAuthValues.InvalidRequest, cancellationToken);
            return;
        }

        var _AccessToken = _TokenFactory.GetOAuthToken();
        var _RefreshToken = _TokenFactory.GetOAuthToken();

        var _AuthenticationMetadata = new Domain.Entities.UserAuthentication()
        {
            AccessToken = _AccessToken,
            ClientApplication = _ClientApplication,
            DateSetUTC = DateTime.UtcNow,
            RefreshToken = _RefreshToken,
            Scopes = string.Join(",", [OAuthValues.WebAppScope.Name]),
            User = _ExistingToken.User
        };

        _PersistenceContext.Remove(_ExistingToken);
        _PersistenceContext.Add(_AuthenticationMetadata);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentAuthorisationGrantedAsync(_AuthenticationMetadata, cancellationToken);
    }

    #endregion Methods

}
