using FluentAssertions;
using Home.Application.Infrastructure.Values;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.OAuth.CreateRefreshGrant;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.OAuth.CreateRefreshGrant;

/// <summary>
/// The refresh token deliberately does not rotate — every tab and device presenting the token it
/// holds must keep working, because rotation is what used to sign the family out. These tests pin
/// that behaviour down.
/// </summary>
public class CreateRefreshGrantInteractorTests
{

    #region Fields

    private readonly ClientApplication m_ClientApplication = new() { ClientApplicationID = 1, Secret = "secret" };
    private readonly Mock<ICreateRefreshGrantOutputPort> m_OutputPort = new();
    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<ITokenFactory> m_TokenFactory = new();
    private readonly User m_User = new() { UserID = 7 };

    #endregion Fields

    #region Methods

    private UserAuthentication BuildToken(string refreshToken, DateTime expiresOnUTC, DateTime? accessTokenSetOnUTC = null)
        => new()
        {
            AccessToken = "access",
            ClientApplication = this.m_ClientApplication,
            DateSetUTC = accessTokenSetOnUTC ?? TestServiceFactory.DefaultNow.UtcDateTime,
            ExpiresOnUTC = expiresOnUTC,
            RefreshToken = refreshToken,
            User = this.m_User
        };

    private Task HandleAsync(string refreshToken, params UserAuthentication[] stored)
    {
        _ = this.m_PersistenceContext.Setup(c => c.GetEntities<ClientApplication>())
            .Returns(new[] { this.m_ClientApplication }.AsQueryable());
        _ = this.m_PersistenceContext.Setup(c => c.GetEntities<UserAuthentication>())
            .Returns(stored.AsQueryable());
        _ = this.m_TokenFactory.Setup(f => f.GetOAuthToken())
            .Returns("new-access");

        var _ServiceFactory = new TestServiceFactory()
            .With(this.m_PersistenceContext.Object)
            .With(this.m_TokenFactory.Object)
            .Build();

        return new CreateRefreshGrantInteractor().HandleAsync(
            new CreateRefreshGrantInputPort(1, "secret", OAuthValues.GrantTypeRefresh.Name, refreshToken),
            this.m_OutputPort.Object,
            _ServiceFactory,
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_KeepsTheRefreshTokenAndSlidesItsExpiry()
    {
        var _Existing = this.BuildToken("refresh", TestServiceFactory.DefaultNow.UtcDateTime.AddDays(30));

        await this.HandleAsync("refresh", _Existing);

        // The whole point of not rotating: the token every other tab and device holds stays good.
        _Existing.RefreshToken.Should().Be("refresh");
        _Existing.ExpiresOnUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime.Add(SessionValues.RefreshTokenLifetime));
        this.m_PersistenceContext.Verify(c => c.Add(It.IsAny<UserAuthentication>()), Times.Never);
        this.m_OutputPort.Verify(
            o => o.PresentAuthorisationGrantedAsync(_Existing, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_KeepsAnAccessTokenThatStillHasLife()
    {
        // Two tabs share this row; re-minting here would invalidate the other tab's token.
        var _Existing = this.BuildToken("refresh", TestServiceFactory.DefaultNow.UtcDateTime.AddDays(30));

        await this.HandleAsync("refresh", _Existing);

        _Existing.AccessToken.Should().Be("access");
        _Existing.DateSetUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
    }

    [Fact]
    public async Task HandleAsync_ReplacesAnAccessTokenThatIsNearlyDead()
    {
        var _Existing = this.BuildToken(
            "refresh",
            TestServiceFactory.DefaultNow.UtcDateTime.AddDays(30),
            TestServiceFactory.DefaultNow.UtcDateTime
                .Subtract(SessionValues.AccessTokenLifetime)
                .Add(TimeSpan.FromMinutes(1)));

        await this.HandleAsync("refresh", _Existing);

        _Existing.AccessToken.Should().Be("new-access");
        // The lifetime is measured from DateSetUTC, so the mint restarts the clock.
        _Existing.DateSetUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
    }

    [Fact]
    public async Task HandleAsync_AnswersALegacySupersededRowWithItsSuccessor()
    {
        // Rows left behind by the rotating scheme this replaced: the device presenting the old
        // token never learnt the successor's, so it is granted the successor rather than refused.
        var _Successor = this.BuildToken("successor-refresh", TestServiceFactory.DefaultNow.UtcDateTime.AddDays(90));
        _Successor.AuthenticationMetadataID = 99;

        var _Existing = this.BuildToken("old-refresh", TestServiceFactory.DefaultNow.UtcDateTime.AddDays(30));
        _Existing.SupersededByAuthenticationMetadataID = 99;
        _Existing.SupersededOnUTC = TestServiceFactory.DefaultNow.UtcDateTime.AddHours(-1);

        await this.HandleAsync("old-refresh", _Existing, _Successor);

        this.m_OutputPort.Verify(
            o => o.PresentAuthorisationGrantedAsync(_Successor, It.IsAny<CancellationToken>()),
            Times.Once);
        this.m_OutputPort.Verify(
            o => o.PresentNotAuthorisedAsync(It.IsAny<OAuthValues>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_PrunesOnlyThatUsersExpiredSessions()
    {
        var _Existing = this.BuildToken("refresh", TestServiceFactory.DefaultNow.UtcDateTime.AddDays(30));
        var _Expired = this.BuildToken("long-dead", TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-1));

        await this.HandleAsync("refresh", _Existing, _Expired);

        this.m_PersistenceContext.Verify(
            c => c.RemoveRange(It.Is<IEnumerable<UserAuthentication>>(t => t.Single() == _Expired)),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RefusesAnExpiredRefreshToken()
    {
        var _Existing = this.BuildToken("refresh", TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-1));

        await this.HandleAsync("refresh", _Existing);

        this.m_OutputPort.Verify(
            o => o.PresentNotAuthorisedAsync(OAuthValues.InvalidRequest, It.IsAny<CancellationToken>()),
            Times.Once);
        this.m_OutputPort.Verify(
            o => o.PresentAuthorisationGrantedAsync(It.IsAny<UserAuthentication>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RefusesUnknownClientCredentialsWithoutThrowing()
    {
        _ = this.m_PersistenceContext.Setup(c => c.GetEntities<ClientApplication>())
            .Returns(Array.Empty<ClientApplication>().AsQueryable());
        _ = this.m_PersistenceContext.Setup(c => c.GetEntities<UserAuthentication>())
            .Returns(Array.Empty<UserAuthentication>().AsQueryable());

        await new CreateRefreshGrantInteractor().HandleAsync(
            new CreateRefreshGrantInputPort(1, "wrong", OAuthValues.GrantTypeRefresh.Name, "refresh"),
            this.m_OutputPort.Object,
            new TestServiceFactory()
                .With(this.m_PersistenceContext.Object)
                .With(this.m_TokenFactory.Object)
                .Build(),
            CancellationToken.None);

        this.m_OutputPort.Verify(
            o => o.PresentNotAuthorisedAsync(OAuthValues.InvalidClient, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion Methods

}
