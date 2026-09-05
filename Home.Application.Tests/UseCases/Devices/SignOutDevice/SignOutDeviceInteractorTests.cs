using FluentAssertions;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Devices.SignOutDevice;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Devices.SignOutDevice;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.Devices.SignOutDevice;

/// <summary>
/// Ending one session from another device. Signing out the device reading the screen is refused:
/// the browser would keep a cookie for a session that no longer exists and only find out at the
/// next refresh, which is worse than not offering it.
/// </summary>
public class SignOutDeviceInteractorTests : InteractorTest
{

    #region Fields

    private readonly SignOutDevicePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private ClientApplication ClientApplication { get; } = new()
    {
        AccessToken = "a-token",
        ClientApplicationID = 1,
        Name = "Home Web App",
        Secret = "a-secret"
    };

    private UserAuthentication BuildDevice(long authenticationMetadataID, User user)
        => new()
        {
            AccessToken = $"access-{authenticationMetadataID}",
            AuthenticationMetadataID = authenticationMetadataID,
            ClientApplication = this.ClientApplication,
            DateSetUTC = TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-10),
            DeviceLabel = $"Device {authenticationMetadataID}",
            ExpiresOnUTC = TestServiceFactory.DefaultNow.UtcDateTime.AddDays(80),
            RefreshToken = $"refresh-{authenticationMetadataID}",
            User = user
        };

    private Task HandleAsync(long authenticationMetadataID, long? currentSessionID = null)
    {
        _ = this.AuthorisationService.Setup(a => a.GetAuthenticationMetadata())
            .Returns(new AuthenticationMetadata() { AuthenticationMetadataID = currentSessionID, UserID = this.Member.UserID });

        return new SignOutDeviceInteractor().HandleAsync(
            new SignOutDeviceInputPort(authenticationMetadataID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_EndsTheSessionAndRemovesTheRow()
    {
        _ = this.Database.Seed(this.BuildDevice(150, this.Member), this.BuildDevice(151, this.Member));

        await this.HandleAsync(150, currentSessionID: 151);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<UserAuthentication>().Select(a => a.AuthenticationMetadataID).Should().Equal([151]);
    }

    [Fact]
    public async Task HandleAsync_RefusesToSignOutTheDeviceReadingTheScreen()
    {
        _ = this.Database.Seed(this.BuildDevice(150, this.Member));

        await this.HandleAsync(150, currentSessionID: 150);

        _ = this.m_Presenter.Result.Should().BeOfType<ConflictResult>();
        _ = this.Stored<UserAuthentication>().Should().ContainSingle(
            "the browser would keep a cookie for a session that no longer exists");
    }

    [Fact]
    public async Task HandleAsync_WhenTheDeviceBelongsToAnotherHousehold_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(this.BuildDevice(150, this.Member), this.BuildDevice(950, this.Neighbour));

        await this.HandleAsync(950, currentSessionID: 150);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<UserAuthentication>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchDeviceExists_PresentsNotFound()
    {
        _ = this.Database.Seed(this.BuildDevice(150, this.Member));

        await this.HandleAsync(404, currentSessionID: 150);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
