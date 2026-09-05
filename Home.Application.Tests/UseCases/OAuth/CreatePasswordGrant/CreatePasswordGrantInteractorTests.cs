using FluentAssertions;
using Home.Application.Infrastructure.Values;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.OAuth.CreatePasswordGrant;
using Home.Domain.Entities;
using Home.Domain.Services.Users;
using Moq;

namespace Home.Application.Tests.UseCases.OAuth.CreatePasswordGrant;

/// <summary>
/// Signing in with a username and password. The client checks are what these pin: until
/// 4 Sep 2026 this slice looked the client application up by ID alone, so the secret was required
/// to be present and never compared, and anybody who could reach the endpoint could mint a token
/// with client_id=1 without knowing it. CreateRefreshGrant had always compared it.
/// </summary>
public class CreatePasswordGrantInteractorTests
{

    #region Constants

    private const string ClientSecret = "the-client-secret";
    private const string Password = "the-password";
    private const string Username = "member@ours.test";

    #endregion Constants

    #region Fields

    private readonly ClientApplication m_ClientApplication = new() { ClientApplicationID = 1, Secret = ClientSecret };
    private readonly Mock<ICreatePasswordGrantOutputPort> m_OutputPort = new();
    private readonly Mock<IPasswordService> m_PasswordService = new();
    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<ITokenFactory> m_TokenFactory = new();
    private readonly User m_User = new() { Email = Username, UserID = 7 };

    #endregion Fields

    #region Methods

    private Task HandleAsync(long clientID, string clientSecret, string username = Username, string password = Password)
    {
        _ = this.m_PersistenceContext.Setup(c => c.GetEntities<ClientApplication>())
            .Returns(new[] { this.m_ClientApplication }.AsQueryable());
        _ = this.m_PersistenceContext.Setup(c => c.GetEntities<User>())
            .Returns(new[] { this.m_User }.AsQueryable());
        _ = this.m_PersistenceContext.Setup(c => c.GetEntities<UserAuthentication>())
            .Returns(Array.Empty<UserAuthentication>().AsQueryable());
        _ = this.m_PasswordService
            .Setup(p => p.VerifyPasswordAsync(It.IsAny<User>(), Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _ = this.m_TokenFactory.Setup(f => f.GetOAuthToken()).Returns("a-token");

        return new CreatePasswordGrantInteractor().HandleAsync(
            new CreatePasswordGrantInputPort(clientID, clientSecret, "Chrome on Windows", OAuthValues.GrantTypePassword.Name, password, "WebApp", username),
            this.m_OutputPort.Object,
            new TestServiceFactory()
                .With(this.m_PasswordService.Object)
                .With(this.m_PersistenceContext.Object)
                .With(this.m_TokenFactory.Object)
                .Build(),
            CancellationToken.None);
    }

    private void ShouldHaveRefused(OAuthValues expected)
        => this.m_OutputPort.Verify(
            o => o.PresentNotAuthorisedAsync(expected, It.IsAny<CancellationToken>()),
            Times.Once);

    [Fact]
    public async Task HandleAsync_WithTheRightClientAndPassword_GrantsAToken()
    {
        await this.HandleAsync(1, ClientSecret);

        this.m_OutputPort.Verify(
            o => o.PresentAuthorisationGrantedAsync(It.IsAny<UserAuthentication>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NamesTheDeviceOnTheSessionItCreates()
    {
        await this.HandleAsync(1, ClientSecret);

        this.m_OutputPort.Verify(
            o => o.PresentAuthorisationGrantedAsync(
                It.Is<UserAuthentication>(a => a.DeviceLabel == "Chrome on Windows"),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "a signed-in devices screen showing 24 unnamed rows is no better than showing token IDs");
    }

    [Fact]
    public async Task HandleAsync_WhenTheClientSecretIsWrong_RefusesTheClientRatherThanCheckingThePassword()
    {
        await this.HandleAsync(1, "not-the-client-secret");

        this.ShouldHaveRefused(OAuthValues.InvalidClient);

        this.m_PasswordService.Verify(
            p => p.VerifyPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "a caller who cannot prove which client it is has no business testing passwords");
    }

    [Fact]
    public async Task HandleAsync_WhenNoClientApplicationRowExists_RefusesTheClient()
    {
        await this.HandleAsync(999, ClientSecret);

        this.ShouldHaveRefused(OAuthValues.InvalidClient);
    }

    [Fact]
    public async Task HandleAsync_WhenTheClientIsRightButThePasswordIsWrong_SaysSoRatherThanBlamingTheClient()
    {
        await this.HandleAsync(1, ClientSecret, password: "the-wrong-password");

        this.ShouldHaveRefused(OAuthValues.InvalidUsernameOrPassword);
    }

    [Fact]
    public async Task HandleAsync_WhenNobodyHasThatUsername_ReadsTheSameAsAWrongPassword()
    {
        await this.HandleAsync(1, ClientSecret, username: "nobody@example.test");

        this.ShouldHaveRefused(
            OAuthValues.InvalidUsernameOrPassword);
    }

    #endregion Methods

}
