using FluentAssertions;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Support.CreateFeedback;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.Support.CreateFeedback;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Support.CreateFeedback;

/// <summary>
/// Sending feedback. The web app says what was written and where the person was; who sent it, for
/// which household, when and from which device all come from the request's own session.
/// </summary>
public class CreateFeedbackInteractorTests : InteractorTest
{

    #region Fields

    private readonly CreateFeedbackPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Properties

    private ClientApplication ClientApplication { get; } = new()
    {
        AccessToken = "a-token",
        ClientApplicationID = 1,
        Name = "Home Web App",
        Secret = "a-secret"
    };

    #endregion Properties

    #region Methods

    private UserAuthentication BuildSession(long authenticationMetadataID, User user, string? deviceLabel)
        => new()
        {
            AccessToken = $"access-{authenticationMetadataID}",
            AuthenticationMetadataID = authenticationMetadataID,
            ClientApplication = this.ClientApplication,
            DateSetUTC = TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-10),
            DeviceLabel = deviceLabel,
            ExpiresOnUTC = TestServiceFactory.DefaultNow.UtcDateTime.AddDays(80),
            RefreshToken = $"refresh-{authenticationMetadataID}",
            User = user
        };

    private Task HandleAsync(CreateFeedbackInputPort inputPort, long? sessionID)
    {
        _ = this.AuthorisationService.Setup(a => a.GetAuthenticationMetadata())
            .Returns(new AuthenticationMetadata() { AuthenticationMetadataID = sessionID, UserID = this.SignedInUser.UserID });

        return new CreateFeedbackInteractor().HandleAsync(
            inputPort,
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_KeepsNoPageWhenNoneWasSent()
    {
        _ = this.Database.Seed(this.BuildSession(150, this.Member, "Safari on iPad"));

        await this.HandleAsync(new CreateFeedbackInputPort("A pantry list", FeedbackKind.Idea, "   ", null, null), 150);

        var _Stored = this.Stored<Feedback>().Single();

        _ = _Stored.Page.Should().BeNull();
        _ = _Stored.ScreenHeight.Should().BeNull();
        _ = _Stored.ScreenWidth.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_NamesNoDeviceWhenTheSessionIsNotTheSenders()
    {
        _ = this.Database.Seed(
            this.BuildSession(150, this.Member, "Safari on iPad"),
            this.BuildSession(950, this.Neighbour, "Chrome on Windows"));

        await this.HandleAsync(new CreateFeedbackInputPort("A pantry list", FeedbackKind.Idea, "/", 800, 1280), 950);

        _ = this.Stored<Feedback>().Single().DeviceLabel.Should().BeNull("another household's session says nothing about where this came from");
    }

    [Fact]
    public async Task HandleAsync_RecordsTheFeedbackWithWhoSentItAndTheirDevice()
    {
        _ = this.Database.Seed(
            this.BuildSession(150, this.Member, "Safari on iPad"),
            this.BuildSession(950, this.Neighbour, "Chrome on Windows"));

        await this.HandleAsync(new CreateFeedbackInputPort("  The timer is too quiet  ", FeedbackKind.Problem, "/recipes/12/cook", 768, 1024), 150);

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Stored = this.Stored<Feedback>().Single();

        _ = _Stored.Content.Should().Be("The timer is too quiet");
        _ = _Stored.CreatedOnUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
        _ = _Stored.DeviceLabel.Should().Be("Safari on iPad");
        _ = _Stored.Kind.Should().Be(FeedbackKind.Problem);
        _ = _Stored.Page.Should().Be("/recipes/12/cook");
        _ = _Stored.ScreenHeight.Should().Be(768);
        _ = _Stored.ScreenWidth.Should().Be(1024);
        _ = this.Stored<Feedback>().Count(f => f.Household.HouseholdID == OurHouseholdID).Should().Be(1);
        _ = this.Stored<Feedback>().Count(f => f.User != null && f.User.UserID == this.Member.UserID).Should().Be(1);
    }

    #endregion Methods

}
