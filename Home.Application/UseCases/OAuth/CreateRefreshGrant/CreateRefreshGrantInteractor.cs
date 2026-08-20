using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.Values;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.OAuth.CreateRefreshGrant;

/// <summary>
/// Issues a fresh access token against the device's existing session row. The refresh token is
/// deliberately <em>not</em> rotated.
/// <para>
/// Rotation was tried and removed. Every Blazor circuit, every open tab and every device holds its
/// own copy of the stored token, and a rotating scheme makes the second one to arrive a suspected
/// leak: a restart, a reload or a tablet waking up all present a token that has already been spent.
/// Widening the grace window only moved the failure. Worse, the leak response revoked every row
/// belonging to the user, so one stale copy signed out the whole household. A session now ends for
/// exactly two reasons — it expired, or somebody signed out.
/// </para>
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

        // Rows left behind by the rotating scheme this replaced. Their holder never learnt the
        // successor's refresh token, so handing the successor back is the only answer that does
        // not cost somebody a password.
        var _Session = this.ResolveSupersededSession(_ExistingToken, _PersistenceContext) ?? _ExistingToken;

        // Only replaced when nearly dead. Every tab on a device shares this row, so an
        // always-minting refresh would have two tabs invalidating each other's token; below the
        // floor they converge on the same one. The lifetime is measured from DateSetUTC, so the
        // two move together.
        if (_Session.DateSetUTC.Add(SessionValues.AccessTokenLifetime).Subtract(SessionValues.AccessTokenReissueFloor) <= _Now)
        {
            _Session.AccessToken = _TokenFactory.GetOAuthToken();
            _Session.DateSetUTC = _Now;
        }

        _Session.ExpiresOnUTC = _Now.Add(SessionValues.RefreshTokenLifetime);
        _Session.LastUsedOnUTC = _Now;

        this.PruneExpiredSessions(_ExistingToken.User, _Now, _PersistenceContext);

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentAuthorisationGrantedAsync(_Session, cancellationToken);
    }

    /// <summary>
    /// Only rows whose own expiry has passed, and only for the user doing the refreshing. Nothing
    /// here may touch a row another device is still using.
    /// </summary>
    private void PruneExpiredSessions(User user, DateTime nowUTC, IPersistenceContext persistenceContext)
    {
        var _Expired = persistenceContext.GetEntities<UserAuthentication>()
            .Where(am => am.User.UserID == user.UserID && am.ExpiresOnUTC <= nowUTC)
            .ToList();

        persistenceContext.RemoveRange(_Expired);
    }

    /// <summary>
    /// The successor is read through a projection rather than a find, so its user and client are
    /// loaded — the presenter reads both when it answers.
    /// </summary>
    private UserAuthentication? ResolveSupersededSession(UserAuthentication existingToken, IPersistenceContext persistenceContext)
    {
        if (existingToken.SupersededOnUTC == null || existingToken.SupersededByAuthenticationMetadataID == null)
            return null;

        return persistenceContext.GetEntities<UserAuthentication>()
            .Where(am => am.AuthenticationMetadataID == existingToken.SupersededByAuthenticationMetadataID.Value)
            .Select(am => new
            {
                AuthenticationMetadata = am,
                am.ClientApplication,
                am.User
            })
            .SingleOrDefault()
            ?.AuthenticationMetadata;
    }

    #endregion Methods

}
