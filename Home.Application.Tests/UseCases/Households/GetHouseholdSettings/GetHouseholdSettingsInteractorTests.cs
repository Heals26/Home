using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Households.GetHouseholdSettings;
using Home.WebApi.Presenters.Households.GetHouseholdSettings;
using Home.WebApi.UseCases.Households.GetHouseholdSettings;
using System.Text.Json;

namespace Home.Application.Tests.UseCases.Households.GetHouseholdSettings;

/// <summary>
/// The Settings screen's own read. The only slice that answers entirely from the authorised
/// household without touching the database, and the only one holding a secret it must not return.
/// </summary>
public class GetHouseholdSettingsInteractorTests : InteractorTest
{

    #region Constants

    private const string StoredToken = "c0ffee-lifx-token";

    #endregion Constants

    #region Fields

    private readonly GetHouseholdSettingsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Task HandleAsync()
        => new GetHouseholdSettingsInteractor().HandleAsync(
            new GetHouseholdSettingsInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_ReturnsTheSignedInHouseholdsOwnSettings()
    {
        this.Ours.Latitude = -37.8136;
        this.Ours.Longitude = 144.9631;

        await this.HandleAsync();

        var _Response = Ok<GetHouseholdSettingsApiResponse>(this.m_Presenter);

        _ = _Response.HouseholdID.Should().Be(OurHouseholdID);
        _ = _Response.Name.Should().Be("Ours");
        _ = _Response.Latitude.Should().Be(-37.8136);
        _ = _Response.Longitude.Should().Be(144.9631);
    }

    [Fact]
    public async Task HandleAsync_SaysALightsTokenIsStoredWithoutHandingItBack()
    {
        this.Ours.LifxApiToken = StoredToken;

        await this.HandleAsync();

        var _Response = Ok<GetHouseholdSettingsApiResponse>(this.m_Presenter);

        _ = _Response.HasLifxApiToken.Should().BeTrue();
        _ = JsonSerializer.Serialize(_Response).Should()
            .NotContain(StoredToken, "the token never leaves the server, only the fact that there is one");
    }

    [Fact]
    public async Task HandleAsync_WhenNoLightsTokenIsStored_SaysSo()
    {
        this.Ours.LifxApiToken = null;

        await this.HandleAsync();

        _ = Ok<GetHouseholdSettingsApiResponse>(this.m_Presenter).HasLifxApiToken.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_AnswersForWhicheverHouseholdIsSignedIn()
    {
        this.SignedInHousehold = this.Theirs;

        await this.HandleAsync();

        _ = Ok<GetHouseholdSettingsApiResponse>(this.m_Presenter).HouseholdID.Should().Be(TheirHouseholdID);
    }

    #endregion Methods

}
