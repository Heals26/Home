using FluentAssertions;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Lights.GetLights;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.Lights.GetLights;

public class GetLightsInteractorTests
{

    #region Fields

    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<IAuthorisationService> m_AuthorisationService = new();
    private readonly Mock<IGetLightsOutputPort> m_OutputPort = new();
    private readonly Household m_Household = new() { HouseholdID = 42 };

    #endregion Fields

    #region Methods

    private LightGroup BuildGroup(string name, int sequence, long householdID = 42, params string[] lightNames)
    {
        var _Group = new LightGroup()
        {
            LightGroupID = sequence + 1,
            Name = name,
            Sequence = sequence,
            Lights = [],
            Location = new LightLocation()
            {
                Name = "Home",
                Household = householdID == 42 ? this.m_Household : new Household() { HouseholdID = householdID }
            }
        };

        foreach (var _LightName in lightNames)
            _Group.Lights.Add(new Light() { ID = _LightName, Name = _LightName, Group = _Group, IsConnected = true });

        return _Group;
    }

    private Task HandleAsync(params LightGroup[] groups)
    {
        _ = this.m_AuthorisationService.Setup(a => a.GetHousehold()).Returns(this.m_Household);
        _ = this.m_PersistenceContext.Setup(c => c.GetEntities<LightGroup>()).Returns(groups.AsQueryable());

        return new GetLightsInteractor().HandleAsync(
            new GetLightsInputPort(),
            this.m_OutputPort.Object,
            new TestServiceFactory()
                .With(this.m_PersistenceContext.Object)
                .With(this.m_AuthorisationService.Object)
                .Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_PresentsGroupsInSequenceOrderNotAlphabetical()
    {
        IReadOnlyList<LightGroup>? _Presented = null;
        this.m_OutputPort
            .Setup(o => o.PresentLightsAsync(It.IsAny<IReadOnlyList<LightGroup>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<LightGroup>, CancellationToken>((g, _) => _Presented = g);

        await this.HandleAsync(
            BuildGroup("Zebra Room", 0),
            BuildGroup("Attic", 1));

        _Presented.Should().NotBeNull();
        _Presented!.Select(g => g.Name).Should().ContainInOrder("Zebra Room", "Attic");
    }

    [Fact]
    public async Task HandleAsync_ExcludesGroupsBelongingToAnotherHousehold()
    {
        IReadOnlyList<LightGroup>? _Presented = null;
        this.m_OutputPort
            .Setup(o => o.PresentLightsAsync(It.IsAny<IReadOnlyList<LightGroup>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<LightGroup>, CancellationToken>((g, _) => _Presented = g);

        await this.HandleAsync(
            BuildGroup("Ours", 0),
            BuildGroup("Someone Else's", 1, householdID: 99));

        _Presented!.Should().HaveCount(1);
        _Presented[0].Name.Should().Be("Ours");
    }

    [Fact]
    public async Task HandleAsync_WhenNothingHasBeenSynced_PresentsAnEmptyList()
    {
        await this.HandleAsync();

        this.m_OutputPort.Verify(
            o => o.PresentLightsAsync(
                It.Is<IReadOnlyList<LightGroup>>(g => g.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CarriesTheLightsBelongingToEachGroup()
    {
        IReadOnlyList<LightGroup>? _Presented = null;
        this.m_OutputPort
            .Setup(o => o.PresentLightsAsync(It.IsAny<IReadOnlyList<LightGroup>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<LightGroup>, CancellationToken>((g, _) => _Presented = g);

        await this.HandleAsync(BuildGroup("Bedroom", 0, 42, "bulb-a", "bulb-b"));

        _Presented![0].Lights.Select(l => l.ID).Should().BeEquivalentTo(["bulb-a", "bulb-b"]);
    }

    #endregion Methods

}
