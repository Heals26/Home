using FluentAssertions;
using Home.Application.Services.Lights;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.LightGroups.SetLightGroupState;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.LightGroups.SetLightGroupState;

public class SetLightGroupStateInteractorTests
{

    #region Fields

    private readonly Mock<ILightService> m_LightService = new();
    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<IAuthorisationService> m_AuthorisationService = new();
    private readonly Mock<ISetLightGroupStateOutputPort> m_OutputPort = new();
    private readonly Household m_Household = new() { HouseholdID = 42 };

    #endregion Fields

    #region Methods

    private LightGroup BuildGroup(long id = 1, long householdID = 42, params Light[] lights)
    {
        var _Group = new LightGroup()
        {
            LightGroupID = id,
            Name = "Living Room",
            Lights = [.. lights],
            Location = new LightLocation()
            {
                Name = "Home",
                Household = householdID == 42 ? this.m_Household : new Household() { HouseholdID = householdID }
            }
        };

        foreach (var _Light in lights)
            _Light.Group = _Group;

        return _Group;
    }

    private static Light BuildLight(string id, bool isConnected = true)
        => new() { ID = id, Name = id, IsConnected = isConnected, IsOn = false, Brightness = 0.1d };

    private Task HandleAsync(SetLightGroupStateInputPort inputPort, params LightGroup[] groups)
    {
        _ = this.m_AuthorisationService.Setup(a => a.GetHousehold()).Returns(this.m_Household);
        _ = this.m_PersistenceContext.Setup(c => c.GetEntities<LightGroup>()).Returns(groups.AsQueryable());

        return new SetLightGroupStateInteractor().HandleAsync(
            inputPort,
            this.m_OutputPort.Object,
            new TestServiceFactory()
                .With(this.m_PersistenceContext.Object)
                .With(this.m_AuthorisationService.Object)
                .With(this.m_LightService.Object)
                .Build(),
            CancellationToken.None);
    }

    private static SetLightGroupStateInputPort PowerOn(long groupID = 1)
        => new(groupID, new(true), new(), new(), new(), new());

    private void SetupResult(LightCommandResult result)
        => this.m_LightService
            .Setup(s => s.SetGroupStateAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<LightStateChange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

    [Fact]
    public async Task HandleAsync_SendsEveryConnectedLightInOneCall()
    {
        IReadOnlyCollection<string>? _Sent = null;
        this.m_LightService
            .Setup(s => s.SetGroupStateAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<LightStateChange>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<string>, LightStateChange, CancellationToken>((ids, _, _) => _Sent = ids)
            .ReturnsAsync(LightCommandResult.Applied);

        await this.HandleAsync(PowerOn(), this.BuildGroup(1, 42, BuildLight("a"), BuildLight("b"), BuildLight("c")));

        _Sent.Should().BeEquivalentTo(["a", "b", "c"]);

        this.m_LightService.Verify(
            s => s.SetGroupStateAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<LightStateChange>(), It.IsAny<CancellationToken>()),
            Times.Once, "a room should cost one provider call, not one per bulb");
    }

    [Fact]
    public async Task HandleAsync_ExcludesOfflineLightsFromTheSelector()
    {
        IReadOnlyCollection<string>? _Sent = null;
        this.m_LightService
            .Setup(s => s.SetGroupStateAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<LightStateChange>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<string>, LightStateChange, CancellationToken>((ids, _, _) => _Sent = ids)
            .ReturnsAsync(LightCommandResult.Applied);

        await this.HandleAsync(PowerOn(), this.BuildGroup(1, 42, BuildLight("a"), BuildLight("offline", isConnected: false)));

        _Sent.Should().BeEquivalentTo(["a"]);
    }

    [Fact]
    public async Task HandleAsync_OnSuccess_UpdatesTheCachedStateOfEveryLightSent()
    {
        this.SetupResult(LightCommandResult.Applied);

        var _A = BuildLight("a");
        var _B = BuildLight("b");

        await this.HandleAsync(
            new SetLightGroupStateInputPort(1, new(true), new(0.6d), new(), new(), new()),
            this.BuildGroup(1, 42, _A, _B));

        _A.IsOn.Should().BeTrue();
        _B.IsOn.Should().BeTrue();
        _A.Brightness.Should().Be(0.6d);
        _B.Brightness.Should().Be(0.6d);
        this.m_PersistenceContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTheProviderIsUnreachable_LeavesTheCacheAlone()
    {
        this.SetupResult(LightCommandResult.Unavailable);

        var _A = BuildLight("a");

        await this.HandleAsync(PowerOn(), this.BuildGroup(1, 42, _A));

        _A.IsOn.Should().BeFalse();
        this.m_OutputPort.Verify(o => o.PresentLightsUnavailableAsync(It.IsAny<CancellationToken>()), Times.Once);
        this.m_PersistenceContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenEveryLightIsOffline_SucceedsWithoutCallingTheProvider()
    {
        await this.HandleAsync(PowerOn(), this.BuildGroup(1, 42, BuildLight("a", isConnected: false)));

        this.m_OutputPort.Verify(o => o.PresentLightGroupStateSetAsync(It.IsAny<CancellationToken>()), Times.Once);

        this.m_LightService.Verify(
            s => s.SetGroupStateAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<LightStateChange>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenNothingWasSet_ShortCircuits()
    {
        await this.HandleAsync(new SetLightGroupStateInputPort(1, new(), new(), new(), new(), new()));

        this.m_OutputPort.Verify(o => o.PresentNothingToChangeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WillNotTouchAnotherHouseholdsGroup()
    {
        await this.HandleAsync(PowerOn(), this.BuildGroup(1, householdID: 99, lights: BuildLight("a")));

        this.m_OutputPort.Verify(
            o => o.PresentLightGroupNotFoundAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion Methods

}
