using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Households.UpdateHouseholdSettings;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Households.UpdateHouseholdSettings;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Households.UpdateHouseholdSettings;

/// <summary>
/// The Settings screen's write. It edits the authorised household directly rather than looking one
/// up by ID, which is what makes it impossible to point at somebody else's.
/// </summary>
public class UpdateHouseholdSettingsInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateHouseholdSettingsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Task HandleAsync(
        PropertyChangeTracker<double?> latitude = default,
        PropertyChangeTracker<string> lifxApiToken = default,
        PropertyChangeTracker<double?> longitude = default,
        PropertyChangeTracker<string> name = default)
        => new UpdateHouseholdSettingsInteractor().HandleAsync(
            new UpdateHouseholdSettingsInputPort(latitude, lifxApiToken, longitude, name),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RenamesTheHouseholdAndSavesIt()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync(name: new("  The Healys  "));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<Household>().Single(h => h.HouseholdID == OurHouseholdID).Name.Should().Be("The Healys");
    }

    [Fact]
    public async Task HandleAsync_SetsWhereTheHouseholdIs()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync(latitude: new(-37.8136), longitude: new(144.9631));

        var _Stored = this.Stored<Household>().Single(h => h.HouseholdID == OurHouseholdID);

        _ = _Stored.Latitude.Should().Be(-37.8136);
        _ = _Stored.Longitude.Should().Be(144.9631);
    }

    [Fact]
    public async Task HandleAsync_StoresALightsToken()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync(lifxApiToken: new("  c0ffee-token  "));

        _ = this.Stored<Household>().Single(h => h.HouseholdID == OurHouseholdID).LifxApiToken.Should().Be("c0ffee-token");
    }

    [Fact]
    public async Task HandleAsync_TreatsAnEmptyTokenAsDisconnectingTheLights()
    {
        this.Ours.LifxApiToken = "c0ffee-token";

        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync(lifxApiToken: new("   "));

        _ = this.Stored<Household>().Single(h => h.HouseholdID == OurHouseholdID).LifxApiToken.Should().BeNull(
            "an empty token is a deliberate disconnect, not a blank one worth storing");
    }

    [Fact]
    public async Task HandleAsync_WhenOnlyTheNameIsSent_LeavesTheLocationAndTokenAlone()
    {
        this.Ours.Latitude = -37.8136;
        this.Ours.LifxApiToken = "c0ffee-token";

        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync(name: new("The Healys"));

        var _Stored = this.Stored<Household>().Single(h => h.HouseholdID == OurHouseholdID);

        _ = _Stored.Latitude.Should().Be(-37.8136);
        _ = _Stored.LifxApiToken.Should().Be("c0ffee-token");
    }

    [Fact]
    public async Task HandleAsync_NeverTouchesAnotherHousehold()
    {
        _ = this.Database.Seed(this.Ours, this.Theirs);

        await this.HandleAsync(name: new("Renamed"));

        _ = this.Stored<Household>().Single(h => h.HouseholdID == TheirHouseholdID).Name.Should().Be(
            "Theirs",
            "the slice edits the authorised household rather than one named by the caller");
    }

    #endregion Methods

}
