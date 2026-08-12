using FluentAssertions;
using Home.Application.Services.Lights;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Lights.SyncLights;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.Lights.SyncLights;

public class SyncLightsInteractorTests
{

    #region Fields

    private readonly Mock<ILightService> m_LightService = new();
    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<IAuthorisationService> m_AuthorisationService = new();
    private readonly Mock<ISyncLightsOutputPort> m_OutputPort = new();
    private readonly Household m_Household = new() { HouseholdID = 42 };
    private readonly List<object> m_Added = [];

    #endregion Fields

    #region Methods

    private static LightSnapshot Snapshot(
        string id,
        string label = "Bedside",
        string groupID = "grp-1",
        string groupName = "Bedroom",
        bool isOn = true,
        double brightness = 0.8d)
        => new(id, label, groupID, groupName, "loc-1", "Home", true, isOn, brightness, 120d, 1d, 3500);

    private LightLocation BuildLocation(params LightGroup[] groups)
    {
        var _Location = new LightLocation()
        {
            ID = "loc-1",
            Name = "Home",
            Household = this.m_Household,
            Groups = [.. groups]
        };

        foreach (var _Group in groups)
            _Group.Location = _Location;

        return _Location;
    }

    private static LightGroup BuildGroup(string lifxID, string name, params Light[] lights)
    {
        var _Group = new LightGroup() { ID = lifxID, Name = name, Lights = [.. lights] };

        foreach (var _Light in lights)
            _Light.Group = _Group;

        return _Group;
    }

    private Task HandleAsync(LightSnapshot[]? snapshots, params LightLocation[] existing)
    {
        _ = this.m_AuthorisationService.Setup(a => a.GetHousehold()).Returns(this.m_Household);
        _ = this.m_LightService
            .Setup(s => s.GetLightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshots);
        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<LightLocation>())
            .Returns(existing.AsQueryable());

        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<Light>()))
            .Callback<Light>(this.m_Added.Add);
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<LightGroup>()))
            .Callback<LightGroup>(this.m_Added.Add);
        this.m_PersistenceContext
            .Setup(c => c.Add(It.IsAny<LightLocation>()))
            .Callback<LightLocation>(this.m_Added.Add);

        return new SyncLightsInteractor().HandleAsync(
            new SyncLightsInputPort(),
            this.m_OutputPort.Object,
            new TestServiceFactory()
                .With(this.m_PersistenceContext.Object)
                .With(this.m_AuthorisationService.Object)
                .With(this.m_LightService.Object)
                .Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_WhenTheProviderIsUnreachable_PresentsUnavailableAndWritesNothing()
    {
        await this.HandleAsync(null);

        this.m_OutputPort.Verify(o => o.PresentLightsUnavailableAsync(It.IsAny<CancellationToken>()), Times.Once);
        this.m_PersistenceContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_OnFirstSync_SeedsTheLocationGroupAndLight()
    {
        await this.HandleAsync([Snapshot("bulb-1")]);

        this.m_Added.OfType<LightLocation>().Should().HaveCount(1);
        this.m_Added.OfType<LightGroup>().Should().HaveCount(1);
        this.m_Added.OfType<Light>().Should().HaveCount(1);

        this.m_Added.OfType<LightGroup>().Single().Name.Should().Be("Bedroom");
        this.m_Added.OfType<Light>().Single().ID.Should().Be("bulb-1");

        this.m_OutputPort.Verify(
            o => o.PresentLightsSyncedAsync(1, 0, 0, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ForAKnownBulb_RefreshesNameAndStateWithoutAddingIt()
    {
        var _Light = new Light() { ID = "bulb-1", Name = "Old Name", IsOn = false, Brightness = 0.1d };
        var _Location = this.BuildLocation(BuildGroup("grp-1", "Bedroom", _Light));

        await this.HandleAsync([Snapshot("bulb-1", label: "New Name", brightness: 0.9d)], _Location);

        _Light.Name.Should().Be("New Name");
        _Light.IsOn.Should().BeTrue();
        _Light.Brightness.Should().Be(0.9d);
        _Light.StateUpdatedUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime, "the fake clock is fixed, so this is exact");

        this.m_Added.OfType<Light>().Should().BeEmpty();
        this.m_OutputPort.Verify(
            o => o.PresentLightsSyncedAsync(0, 1, 0, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DoesNotMoveAKnownBulbBackIntoTheProvidersGroup()
    {
        // The whole point of owning grouping in Home: the user moved this bulb to "Reading Nook",
        // LIFX still calls it "Bedroom", and a sync must not undo that.
        var _Light = new Light() { ID = "bulb-1", Name = "Bedside" };
        var _HomeGroup = BuildGroup(null!, "Reading Nook", _Light);
        var _Location = this.BuildLocation(_HomeGroup);

        await this.HandleAsync([Snapshot("bulb-1", groupID: "grp-1", groupName: "Bedroom")], _Location);

        _Light.Group.Name.Should().Be("Reading Nook");
        this.m_Added.OfType<LightGroup>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_RemovesBulbsThatHaveLeftTheAccount()
    {
        var _Gone = new Light() { ID = "bulb-gone", Name = "Old Lamp" };
        var _Kept = new Light() { ID = "bulb-1", Name = "Bedside" };
        var _Location = this.BuildLocation(BuildGroup("grp-1", "Bedroom", _Gone, _Kept));

        var _Removed = new List<Light>();
        this.m_PersistenceContext
            .Setup(c => c.RemoveRange(It.IsAny<IEnumerable<Light>>()))
            .Callback<IEnumerable<Light>>(l => _Removed.AddRange(l));

        await this.HandleAsync([Snapshot("bulb-1")], _Location);

        _Removed.Select(l => l.ID).Should().BeEquivalentTo(["bulb-gone"]);
        this.m_OutputPort.Verify(
            o => o.PresentLightsSyncedAsync(0, 1, 1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_IgnoresAnotherHouseholdsRecords()
    {
        var _Theirs = new Light() { ID = "bulb-1", Name = "Someone Else's" };
        var _Location = new LightLocation()
        {
            ID = "loc-1",
            Name = "Elsewhere",
            Household = new Household() { HouseholdID = 99 },
            Groups = [BuildGroup("grp-1", "Bedroom", _Theirs)]
        };
        _Location.Groups.Single().Location = _Location;

        await this.HandleAsync([Snapshot("bulb-1")], _Location);

        // Treated as brand new for our household rather than adopting their row.
        this.m_Added.OfType<Light>().Should().HaveCount(1);
        _Theirs.Name.Should().Be("Someone Else's");
    }

    [Fact]
    public async Task HandleAsync_PutsASecondBulbFromTheSameGroupIntoThatOneGroup()
    {
        await this.HandleAsync([Snapshot("bulb-1", "Left"), Snapshot("bulb-2", "Right")]);

        this.m_Added.OfType<LightGroup>().Should().HaveCount(1);
        this.m_Added.OfType<Light>().Should().HaveCount(2);
        this.m_Added.OfType<Light>().Select(l => l.Group.Name).Should().AllBe("Bedroom");
    }

    #endregion Methods

}
