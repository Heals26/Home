using FluentAssertions;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Devices.SignOutOtherDevices;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Devices.SignOutOtherDevices;
using Home.WebApi.UseCases.Devices.SignOutOtherDevices;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.Devices.SignOutOtherDevices;

/// <summary>
/// Ending every session but this one. A household gathers a session per sign-in and nothing prunes
/// them, so this is the only thing in the application that clears the pile, expired rows included.
/// </summary>
public class SignOutOtherDevicesInteractorTests : InteractorTest
{

    #region Fields

    private readonly SignOutOtherDevicesPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private ClientApplication ClientApplication { get; } = new()
    {
        AccessToken = "a-token",
        ClientApplicationID = 1,
        Name = "Home Web App",
        Secret = "a-secret"
    };

    private UserAuthentication BuildDevice(long authenticationMetadataID, User user, DateTime? expiresOnUTC = null)
        => new()
        {
            AccessToken = $"access-{authenticationMetadataID}",
            AuthenticationMetadataID = authenticationMetadataID,
            ClientApplication = this.ClientApplication,
            DateSetUTC = TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-10),
            DeviceLabel = $"Device {authenticationMetadataID}",
            ExpiresOnUTC = expiresOnUTC ?? TestServiceFactory.DefaultNow.UtcDateTime.AddDays(80),
            RefreshToken = $"refresh-{authenticationMetadataID}",
            User = user
        };

    private Task HandleAsync(long? currentSessionID)
    {
        _ = this.AuthorisationService.Setup(a => a.GetAuthenticationMetadata())
            .Returns(new AuthenticationMetadata() { AuthenticationMetadataID = currentSessionID, UserID = this.Member.UserID });

        return new SignOutOtherDevicesInteractor().HandleAsync(
            new SignOutOtherDevicesInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_EndsEverySessionExceptTheOneAsking()
    {
        _ = this.Database.Seed(
            this.BuildDevice(150, this.Member),
            this.BuildDevice(151, this.Member),
            this.BuildDevice(152, this.Member));

        await this.HandleAsync(currentSessionID: 151);

        _ = Ok<SignOutOtherDevicesApiResponse>(this.m_Presenter).SignedOutCount.Should().Be(2);
        _ = this.Stored<UserAuthentication>().Select(a => a.AuthenticationMetadataID).Should().Equal([151]);
    }

    [Fact]
    public async Task HandleAsync_TakesExpiredRowsWithIt()
    {
        _ = this.Database.Seed(
            this.BuildDevice(150, this.Member, expiresOnUTC: TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-30)),
            this.BuildDevice(151, this.Member));

        await this.HandleAsync(currentSessionID: 151);

        _ = this.Stored<UserAuthentication>().Should().ContainSingle(
            "nothing else in the application ever prunes an expired session");
    }

    [Fact]
    public async Task HandleAsync_NeverTouchesAnotherHouseholdsSessions()
    {
        _ = this.Database.Seed(
            this.BuildDevice(150, this.Member),
            this.BuildDevice(151, this.Member),
            this.BuildDevice(950, this.Neighbour));

        await this.HandleAsync(currentSessionID: 151);

        _ = Ok<SignOutOtherDevicesApiResponse>(this.m_Presenter).SignedOutCount.Should().Be(1);
        _ = this.Stored<UserAuthentication>().Select(a => a.AuthenticationMetadataID).Should().BeEquivalentTo([151L, 950L]);
    }

    [Fact]
    public async Task HandleAsync_WhenThereIsNothingElseSignedIn_SaysNoneRatherThanFailing()
    {
        _ = this.Database.Seed(this.BuildDevice(150, this.Member));

        await this.HandleAsync(currentSessionID: 150);

        _ = Ok<SignOutOtherDevicesApiResponse>(this.m_Presenter).SignedOutCount.Should().Be(0);
        _ = this.Stored<UserAuthentication>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenTheCallerCannotBePlaced_RefusesRatherThanSigningItselfOutToo()
    {
        _ = this.Database.Seed(this.BuildDevice(150, this.Member), this.BuildDevice(151, this.Member));

        await this.HandleAsync(currentSessionID: null);

        _ = this.m_Presenter.Result.Should().BeOfType<ConflictResult>();
        _ = this.Stored<UserAuthentication>().Should().HaveCount(2);
    }

    #endregion Methods

}
