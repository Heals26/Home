using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Services.Lights;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Lights.SetLightState;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.Lights.SetLightState;

public class SetLightStateInteractorTests
{

    #region Fields

    private readonly Mock<ILightService> m_LightService = new();
    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<IAuthorisationService> m_AuthorisationService = new();
    private readonly Mock<ISetLightStateOutputPort> m_OutputPort = new();
    private readonly Household m_Household = new() { HouseholdID = 42 };
    private readonly Light m_Light;

    #endregion Fields

    #region Constructors

    public SetLightStateInteractorTests()
    {
        this.m_Light = new Light()
        {
            ID = "d073d5",
            Name = "Bedside",
            IsConnected = true,
            IsOn = false,
            Brightness = 0.2d,
            Saturation = 1d,
            Hue = 300d
        };

        this.m_Light.Group = new LightGroup()
        {
            Name = "Bedroom",
            Lights = [this.m_Light],
            Location = new LightLocation() { Name = "Home", Household = this.m_Household }
        };
    }

    #endregion Constructors

    #region Methods

    private Task HandleAsync(SetLightStateInputPort inputPort, params Light[] stored)
    {
        _ = this.m_AuthorisationService.Setup(a => a.GetHousehold()).Returns(this.m_Household);
        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<Light>())
            .Returns((stored.Length == 0 ? new[] { this.m_Light } : stored).AsQueryable());

        return new SetLightStateInteractor().HandleAsync(
            inputPort,
            this.m_OutputPort.Object,
            new TestServiceFactory()
                .With(this.m_PersistenceContext.Object)
                .With(this.m_AuthorisationService.Object)
                .With(this.m_LightService.Object)
                .Build(),
            CancellationToken.None);
    }

    private static SetLightStateInputPort Nothing(string lightID = "d073d5")
        => new(lightID, new(), new(), new(), new(), new());

    private static SetLightStateInputPort PowerOn(string lightID = "d073d5")
        => new(lightID, new(true), new(), new(), new(), new());

    private void SetupResult(LightCommandResult result)
        => this.m_LightService
            .Setup(s => s.SetStateAsync(It.IsAny<string>(), It.IsAny<LightStateChange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

    [Fact]
    public async Task HandleAsync_WhenTheProviderApplies_PresentsSuccess()
    {
        this.SetupResult(LightCommandResult.Applied);

        await this.HandleAsync(PowerOn());

        this.m_OutputPort.Verify(o => o.PresentLightStateSetAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNothingWasSet_ShortCircuitsWithoutCallingTheProvider()
    {
        await this.HandleAsync(Nothing());

        this.m_OutputPort.Verify(o => o.PresentNothingToChangeAsync(It.IsAny<CancellationToken>()), Times.Once);

        this.m_LightService.Verify(
            s => s.SetStateAsync(It.IsAny<string>(), It.IsAny<LightStateChange>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenHomeDoesNotKnowTheLight_PresentsNotFoundWithoutCallingTheProvider()
    {
        await this.HandleAsync(PowerOn("missing-bulb"));

        this.m_OutputPort.Verify(
            o => o.PresentLightNotFoundAsync("missing-bulb", It.IsAny<CancellationToken>()),
            Times.Once);

        this.m_LightService.Verify(
            s => s.SetStateAsync(It.IsAny<string>(), It.IsAny<LightStateChange>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTheProviderIsUnreachable_PresentsUnavailableAndLeavesTheCacheAlone()
    {
        this.SetupResult(LightCommandResult.Unavailable);

        await this.HandleAsync(PowerOn());

        this.m_OutputPort.Verify(
            o => o.PresentLightsUnavailableAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        this.m_Light.IsOn.Should().BeFalse();
        this.m_PersistenceContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_OnSuccess_WritesTheChangeIntoHomesCachedCopy()
    {
        this.SetupResult(LightCommandResult.Applied);

        await this.HandleAsync(new SetLightStateInputPort(
            "d073d5", new(true), new(0.75d), new(), new(), new()));

        this.m_Light.IsOn.Should().BeTrue();
        this.m_Light.Brightness.Should().Be(0.75d);
        this.m_Light.StateUpdatedUTC.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        this.m_PersistenceContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SettingKelvinAlsoClearsSaturationInTheCache()
    {
        this.SetupResult(LightCommandResult.Applied);

        await this.HandleAsync(new SetLightStateInputPort(
            "d073d5", new(), new(), new(), new(), new(2700)));

        this.m_Light.Kelvin.Should().Be(2700);
        this.m_Light.Saturation.Should().Be(0d, "kelvin and saturation are mutually exclusive on the wire");
    }

    [Fact]
    public async Task HandleAsync_ForwardsOnlyThePropertiesThatWereSet()
    {
        LightStateChange? _Change = null;
        this.m_LightService
            .Setup(s => s.SetStateAsync(It.IsAny<string>(), It.IsAny<LightStateChange>(), It.IsAny<CancellationToken>()))
            .Callback<string, LightStateChange, CancellationToken>((_, c, _) => _Change = c)
            .ReturnsAsync(LightCommandResult.Applied);

        await this.HandleAsync(new SetLightStateInputPort(
            "d073d5",
            new PropertyChangeTracker<bool>(),
            new PropertyChangeTracker<double>(0.4d),
            new PropertyChangeTracker<double>(),
            new PropertyChangeTracker<double>(),
            new PropertyChangeTracker<int>()));

        _Change.Should().NotBeNull();
        _Change!.Brightness.HasBeenSet.Should().BeTrue();
        _Change.Brightness.Value.Should().Be(0.4d);
        _Change.IsOn.HasBeenSet.Should().BeFalse();
        _Change.Hue.HasBeenSet.Should().BeFalse();
        _Change.Kelvin.HasBeenSet.Should().BeFalse();
    }

    #endregion Methods

}
