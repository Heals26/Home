using FluentAssertions;
using Home.Application.Infrastructure.Values;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.OAuth.CreateRefreshGrant;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.OAuth.CreateRefreshGrant;

public class CreateRefreshGrantInteractorTests
{

    #region Fields

    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<ITokenFactory> m_TokenFactory = new();
    private readonly Mock<ICreateRefreshGrantOutputPort> m_OutputPort = new();
    private readonly ClientApplication m_ClientApplication = new() { ClientApplicationID = 1, Secret = "secret" };
    private readonly User m_User = new() { UserID = 7 };

    #endregion Fields

    #region Methods

    private UserAuthentication BuildToken(string refreshToken, DateTime expiresOnUTC)
        => new()
        {
            AccessToken = "access",
            ClientApplication = this.m_ClientApplication,
            DateSetUTC = TestServiceFactory.DefaultNow.UtcDateTime,
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
        _ = this.m_TokenFactory.SetupSequence(f => f.GetOAuthToken())
            .Returns("new-access")
            .Returns("new-refresh");

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
    public async Task HandleAsync_RotatesTheTokenAndKeepsTheOldRowRatherThanDeletingIt()
    {
        var _Existing = this.BuildToken("old-refresh", TestServiceFactory.DefaultNow.UtcDateTime.AddDays(30));

        UserAuthentication? _Added = null;
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<UserAuthentication>()))
            .Callback<UserAuthentication>(a => _Added = a);

        await this.HandleAsync("old-refresh", _Existing);

        _Added.Should().NotBeNull();
        _Added!.RefreshToken.Should().Be("new-refresh");
        _Added.ExpiresOnUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime.Add(SessionValues.RefreshTokenLifetime));

        // The row must survive so a sibling circuit presenting the same token is answered.
        this.m_PersistenceContext.Verify(c => c.Remove(_Existing), Times.Never);
        _Existing.SupersededOnUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
    }

    [Fact]
    public async Task HandleAsync_WithinTheGraceWindow_ReturnsTheSameSuccessorInsteadOfSigningOut()
    {
        // The exact restart race: two circuits present the one stored token moments apart.
        var _Successor = this.BuildToken("new-refresh", TestServiceFactory.DefaultNow.UtcDateTime.AddDays(90));
        _Successor.AuthenticationMetadataID = 99;

        var _Existing = this.BuildToken("old-refresh", TestServiceFactory.DefaultNow.UtcDateTime.AddDays(30));
        _Existing.SupersededByAuthenticationMetadataID = 99;
        _Existing.SupersededOnUTC = TestServiceFactory.DefaultNow.UtcDateTime.AddSeconds(-5);

        _ = this.m_PersistenceContext.Setup(c => c.Find<UserAuthentication>(99L)).Returns(_Successor);

        await this.HandleAsync("old-refresh", _Existing, _Successor);

        this.m_OutputPort.Verify(
            o => o.PresentAuthorisationGrantedAsync(_Successor, It.IsAny<CancellationToken>()),
            Times.Once);
        this.m_OutputPort.Verify(
            o => o.PresentNotAuthorisedAsync(It.IsAny<OAuthValues>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_OutsideTheGraceWindow_RevokesEverySessionForThatUser()
    {
        var _Successor = this.BuildToken("new-refresh", TestServiceFactory.DefaultNow.UtcDateTime.AddDays(90));
        _Successor.AuthenticationMetadataID = 99;

        var _Existing = this.BuildToken("old-refresh", TestServiceFactory.DefaultNow.UtcDateTime.AddDays(30));
        _Existing.SupersededByAuthenticationMetadataID = 99;
        _Existing.SupersededOnUTC = TestServiceFactory.DefaultNow.UtcDateTime.Subtract(TimeSpan.FromHours(1));

        _ = this.m_PersistenceContext.Setup(c => c.Find<UserAuthentication>(99L)).Returns(_Successor);

        await this.HandleAsync("old-refresh", _Existing, _Successor);

        this.m_PersistenceContext.Verify(
            c => c.RemoveRange(It.Is<IEnumerable<UserAuthentication>>(t => t.Count() == 2)),
            Times.Once);
        this.m_OutputPort.Verify(
            o => o.PresentNotAuthorisedAsync(OAuthValues.InvalidRequest, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RefusesAnExpiredRefreshToken()
    {
        var _Existing = this.BuildToken("old-refresh", TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-1));

        await this.HandleAsync("old-refresh", _Existing);

        this.m_OutputPort.Verify(
            o => o.PresentNotAuthorisedAsync(OAuthValues.InvalidRequest, It.IsAny<CancellationToken>()),
            Times.Once);
        this.m_PersistenceContext.Verify(c => c.Add(It.IsAny<UserAuthentication>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RefusesUnknownClientCredentialsWithoutThrowing()
    {
        _ = this.m_PersistenceContext.Setup(c => c.GetEntities<ClientApplication>())
            .Returns(Array.Empty<ClientApplication>().AsQueryable());
        _ = this.m_PersistenceContext.Setup(c => c.GetEntities<UserAuthentication>())
            .Returns(Array.Empty<UserAuthentication>().AsQueryable());

        await new CreateRefreshGrantInteractor().HandleAsync(
            new CreateRefreshGrantInputPort(1, "wrong", OAuthValues.GrantTypeRefresh.Name, "old-refresh"),
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
