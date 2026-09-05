using FluentAssertions;
using Home.Application.Services.EntityLogic.Lights;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Lights.SyncAllLights;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.Lights.SyncAllLights;

/// <summary>
/// The background refresh, so a light switched at the wall shows up on the board without anyone
/// pressing Sync. Like the schedule runner it covers every household and has no signed-in user,
/// and it only bothers with households that have a provider token stored.
/// </summary>
public class SyncAllLightsInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<ILightSyncLogic> m_LightSyncLogic = new();
    private readonly Mock<ISyncAllLightsOutputPort> m_OutputPort = new();

    #endregion Fields

    #region Methods

    private Task HandleAsync()
        => new SyncAllLightsInteractor().HandleAsync(
            new SyncAllLightsInputPort(),
            this.m_OutputPort.Object,
            this.Services().With(this.m_LightSyncLogic.Object).Build(),
            CancellationToken.None);

    private void SyncReturns(LightSyncResult? result)
        => this.m_LightSyncLogic
            .Setup(l => l.SyncHouseholdAsync(It.IsAny<Household>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

    [Fact]
    public async Task HandleAsync_SyncsEveryHouseholdWithAStoredToken()
    {
        this.Ours.LifxApiToken = "ours-token";
        this.Theirs.LifxApiToken = "theirs-token";

        _ = this.Database.Seed(this.Ours, this.Theirs);
        this.SyncReturns(new LightSyncResult(1, 2, 0));

        await this.HandleAsync();

        this.m_LightSyncLogic.Verify(
            l => l.SyncHouseholdAsync(It.IsAny<Household>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2),
            "there is no signed-in user behind a timer tick, so it covers everybody");

        this.m_OutputPort.Verify(
            o => o.PresentAllLightsSyncedAsync(
                It.Is<List<long>>(ids => ids.Count == 2),
                0,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SkipsAHouseholdWithNoToken()
    {
        this.Ours.LifxApiToken = "ours-token";
        this.Theirs.LifxApiToken = null;

        _ = this.Database.Seed(this.Ours, this.Theirs);
        this.SyncReturns(new LightSyncResult(0, 1, 0));

        await this.HandleAsync();

        this.m_LightSyncLogic.Verify(
            l => l.SyncHouseholdAsync(It.IsAny<Household>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SkipsAHouseholdWhoseTokenIsBlank()
    {
        this.Ours.LifxApiToken = string.Empty;

        _ = this.Database.Seed(this.Ours);
        this.SyncReturns(new LightSyncResult(0, 0, 0));

        await this.HandleAsync();

        this.m_LightSyncLogic.Verify(
            l => l.SyncHouseholdAsync(It.IsAny<Household>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_CountsAHouseholdWhoseProviderCouldNotBeReachedAsUnavailable()
    {
        this.Ours.LifxApiToken = "ours-token";

        _ = this.Database.Seed(this.Ours);
        this.SyncReturns(null);

        await this.HandleAsync();

        this.m_OutputPort.Verify(
            o => o.PresentAllLightsSyncedAsync(
                It.Is<List<long>>(ids => ids.Count == 0),
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNobodyHasAToken_SaysSoWithoutCallingTheProvider()
    {
        _ = this.Database.Seed(this.Ours, this.Theirs);

        await this.HandleAsync();

        this.m_OutputPort.Verify(
            o => o.PresentAllLightsSyncedAsync(
                It.Is<List<long>>(ids => ids.Count == 0),
                0,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion Methods

}
