using FluentAssertions;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Devices.GetDevices;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Devices.GetDevices;
using Home.WebApi.UseCases.Devices.GetDevices;
using Moq;

namespace Home.Application.Tests.UseCases.Devices.GetDevices;

/// <summary>
/// The household's signed-in devices. Expired rows are left out, because the table keeps them
/// forever and a session nobody can sign in with is not a device.
/// </summary>
public class GetDevicesInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetDevicesPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private UserAuthentication BuildDevice(
        long authenticationMetadataID,
        User user,
        string? deviceLabel = "Chrome on Windows",
        DateTime? lastUsedOnUTC = null,
        DateTime? expiresOnUTC = null)
        => new()
        {
            AccessToken = $"access-{authenticationMetadataID}",
            AuthenticationMetadataID = authenticationMetadataID,
            ClientApplication = this.ClientApplication,
            DateSetUTC = TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-10),
            DeviceLabel = deviceLabel,
            ExpiresOnUTC = expiresOnUTC ?? TestServiceFactory.DefaultNow.UtcDateTime.AddDays(80),
            LastUsedOnUTC = lastUsedOnUTC,
            RefreshToken = $"refresh-{authenticationMetadataID}",
            User = user
        };

    private ClientApplication ClientApplication { get; } = new()
    {
        AccessToken = "a-token",
        ClientApplicationID = 1,
        Name = "Home Web App",
        Secret = "a-secret"
    };

    private Task HandleAsync(long? currentSessionID = null)
    {
        _ = this.AuthorisationService.Setup(a => a.GetAuthenticationMetadata())
            .Returns(new AuthenticationMetadata() { AuthenticationMetadataID = currentSessionID, UserID = this.Member.UserID });

        return new GetDevicesInteractor().HandleAsync(
            new GetDevicesInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_NamesEachDeviceAndWhenItWasLastUsed()
    {
        _ = this.Database.Seed(this.BuildDevice(
            150,
            this.Member,
            "Safari on iPad",
            lastUsedOnUTC: TestServiceFactory.DefaultNow.UtcDateTime.AddHours(-2)));

        await this.HandleAsync();

        var _Device = Ok<GetDevicesApiResponse>(this.m_Presenter).Devices.Single();

        _ = _Device.Name.Should().Be("Safari on iPad");
        _ = _Device.LastUsedOnUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime.AddHours(-2));
        _ = _Device.SignedInOnUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-10));
    }

    [Fact]
    public async Task HandleAsync_SaysUnknownForASessionSignedInBeforeLabelsWereCaptured()
    {
        _ = this.Database.Seed(this.BuildDevice(150, this.Member, deviceLabel: null));

        await this.HandleAsync();

        _ = Ok<GetDevicesApiResponse>(this.m_Presenter).Devices.Single().Name.Should().Be(
            "Unknown device",
            "an empty row reads worse than saying the label was never captured");
    }

    [Fact]
    public async Task HandleAsync_LeadsWithTheDeviceUsedMostRecently()
    {
        var _Now = TestServiceFactory.DefaultNow.UtcDateTime;

        _ = this.Database.Seed(
            this.BuildDevice(150, this.Member, "Old tablet", lastUsedOnUTC: _Now.AddDays(-5)),
            this.BuildDevice(151, this.Member, "Kitchen tablet", lastUsedOnUTC: _Now.AddMinutes(-5)),
            this.BuildDevice(152, this.Member, "Phone", lastUsedOnUTC: _Now.AddHours(-3)));

        await this.HandleAsync();

        _ = Ok<GetDevicesApiResponse>(this.m_Presenter).Devices
            .Select(d => d.Name).Should().Equal(["Kitchen tablet", "Phone", "Old tablet"]);
    }

    [Fact]
    public async Task HandleAsync_FallsBackToWhenASessionWasCreatedIfItHasNeverBeenUsed()
    {
        var _Now = TestServiceFactory.DefaultNow.UtcDateTime;

        _ = this.Database.Seed(
            this.BuildDevice(150, this.Member, "Never used", lastUsedOnUTC: null),
            this.BuildDevice(151, this.Member, "Used yesterday", lastUsedOnUTC: _Now.AddDays(-1)));

        await this.HandleAsync();

        _ = Ok<GetDevicesApiResponse>(this.m_Presenter).Devices
            .Select(d => d.Name).Should().Equal(["Used yesterday", "Never used"]);
    }

    [Fact]
    public async Task HandleAsync_LeavesOutSessionsThatHaveExpired()
    {
        var _Now = TestServiceFactory.DefaultNow.UtcDateTime;

        _ = this.Database.Seed(
            this.BuildDevice(150, this.Member, "Live", expiresOnUTC: _Now.AddDays(1)),
            this.BuildDevice(151, this.Member, "Expired", expiresOnUTC: _Now.AddDays(-1)));

        await this.HandleAsync();

        _ = Ok<GetDevicesApiResponse>(this.m_Presenter).Devices
            .Select(d => d.Name).Should().Equal(["Live"]);
    }

    [Fact]
    public async Task HandleAsync_MarksTheDeviceTheScreenIsBeingReadOn()
    {
        _ = this.Database.Seed(this.BuildDevice(150, this.Member), this.BuildDevice(151, this.Member));

        await this.HandleAsync(currentSessionID: 151);

        var _Devices = Ok<GetDevicesApiResponse>(this.m_Presenter).Devices;

        _ = _Devices.Should().ContainSingle(d => d.IsCurrentDevice);
        _ = _Devices.Single(d => d.IsCurrentDevice).AuthenticationMetadataID.Should().Be(151);
    }

    [Fact]
    public async Task HandleAsync_WhenTheCallerCannotBePlaced_MarksNoRowRatherThanTheWrongOne()
    {
        _ = this.Database.Seed(this.BuildDevice(150, this.Member));

        await this.HandleAsync(currentSessionID: null);

        _ = Ok<GetDevicesApiResponse>(this.m_Presenter).Devices.Should().NotContain(d => d.IsCurrentDevice);
    }

    [Fact]
    public async Task HandleAsync_NeverShowsAnotherHouseholdsDevices()
    {
        _ = this.Database.Seed(
            this.BuildDevice(150, this.Member, "Ours"),
            this.BuildDevice(950, this.Neighbour, "Theirs"));

        await this.HandleAsync();

        _ = Ok<GetDevicesApiResponse>(this.m_Presenter).Devices
            .Select(d => d.Name).Should().Equal(["Ours"]);
    }

    #endregion Methods

}
