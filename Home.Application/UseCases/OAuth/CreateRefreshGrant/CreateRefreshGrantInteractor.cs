using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.Values;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.OAuth.CreateRefreshGrant;

/// <summary>
/// Rotates the refresh token, keeping the one just replaced usable for a short grace window.
/// Every device and every Blazor circuit holds the same stored token, so a server restart makes
/// several of them present it at the same moment; rejecting all but the first is what used to
/// sign the family out on every restart. Presenting a rotated token after the window has closed
/// is treated as a leak and revokes the whole session.
/// </summary>
internal class CreateRefreshGrantInteractor : IInteractor<CreateRefreshGrantInputPort, ICreateRefreshGrantOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateRefreshGrantInputPort inputPort,
        ICreateRefreshGrantOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _TokenFactory = serviceFactory.GetService<ITokenFactory>();
        var _Now = serviceFactory.GetService<TimeProvider>().GetUtcNow().UtcDateTime;

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

        // SingleOrDefault, not Single: unknown credentials are an unauthorised caller, not a 500.
        var _ClientApplication = _PersistenceContext.GetEntities<ClientApplication>()
            .SingleOrDefault(ca => ca.ClientApplicationID == inputPort.ClientID && ca.Secret == inputPort.ClientSecret);

        if (_ClientApplication == null)
        {
            await outputPort.PresentNotAuthorisedAsync(OAuthValues.InvalidClient, cancellationToken);
            return;
        }

        var _ExistingToken = _PersistenceContext.GetEntities<UserAuthentication>()
            .Where(am => am.RefreshToken == inputPort.RefreshToken)
            .Select(am => new
            {
                AuthenticationMetadata = am,
                am.User
            })
            .SingleOrDefault()
            ?.AuthenticationMetadata;

        if (_ExistingToken == null || _ExistingToken.ExpiresOnUTC <= _Now)
        {
            await outputPort.PresentNotAuthorisedAsync(OAuthValues.InvalidRequest, cancellationToken);
            return;
        }

        if (_ExistingToken.SupersededOnUTC != null)
        {
            await this.ReplayOrRevokeAsync(_ExistingToken, _Now, _PersistenceContext, outputPort, cancellationToken);
            return;
        }

        var _AuthenticationMetadata = new UserAuthentication()
        {
            AccessToken = _TokenFactory.GetOAuthToken(),
            ClientApplication = _ClientApplication,
            DateSetUTC = _Now,
            DeviceLabel = _ExistingToken.DeviceLabel,
            ExpiresOnUTC = _Now.Add(SessionValues.RefreshTokenLifetime),
            LastUsedOnUTC = _Now,
            RefreshToken = _TokenFactory.GetOAuthToken(),
            Scopes = string.Join(",", [OAuthValues.WebAppScope.Name]),
            User = _ExistingToken.User
        };

        _PersistenceContext.Add(_AuthenticationMetadata);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        // Kept rather than deleted, so a sibling circuit presenting the same token moments later
        // is answered instead of signed out. The identity is only known after the insert.
        _ExistingToken.SupersededByAuthenticationMetadataID = _AuthenticationMetadata.AuthenticationMetadataID;
        _ExistingToken.SupersededOnUTC = _Now;

        this.PruneSpentSessions(_ExistingToken.User, _Now, _PersistenceContext);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentAuthorisationGrantedAsync(_AuthenticationMetadata, cancellationToken);
    }

    /// <summary>
    /// Rows nobody can use again. Without this the table grows by one row per refresh forever.
    /// </summary>
    private void PruneSpentSessions(User user, DateTime nowUTC, IPersistenceContext persistenceContext)
    {
        var _Cutoff = nowUTC.Subtract(SessionValues.RefreshGraceWindow);

        var _Spent = persistenceContext.GetEntities<UserAuthentication>()
            .Where(am => am.User.UserID == user.UserID
                && (am.ExpiresOnUTC <= nowUTC || (am.SupersededOnUTC != null && am.SupersededOnUTC < _Cutoff)))
            .ToList();

        persistenceContext.RemoveRange(_Spent);
    }

    private async Task ReplayOrRevokeAsync(
        UserAuthentication existingToken,
        DateTime nowUTC,
        IPersistenceContext persistenceContext,
        ICreateRefreshGrantOutputPort outputPort,
        CancellationToken cancellationToken)
    {
        var _Successor = existingToken.SupersededByAuthenticationMetadataID == null
            ? null
            : persistenceContext.Find<UserAuthentication>(existingToken.SupersededByAuthenticationMetadataID.Value);

        if (_Successor != null && nowUTC.Subtract(existingToken.SupersededOnUTC!.Value) <= SessionValues.RefreshGraceWindow)
        {
            await outputPort.PresentAuthorisationGrantedAsync(_Successor, cancellationToken);
            return;
        }

        // Outside the window the token should have been long discarded, so its reappearance means
        // a copy is loose. Everything issued to this user goes, including the successor.
        var _Chain = persistenceContext.GetEntities<UserAuthentication>()
            .Where(am => am.User.UserID == existingToken.User.UserID)
            .ToList();

        persistenceContext.RemoveRange(_Chain);
        _ = await persistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentNotAuthorisedAsync(OAuthValues.InvalidRequest, cancellationToken);
    }

    #endregion Methods

}
