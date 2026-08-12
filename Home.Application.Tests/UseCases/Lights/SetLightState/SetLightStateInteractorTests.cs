using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Services.Lights;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Lights.SetLightState;
using Moq;

namespace Home.Application.Tests.UseCases.Lights.SetLightState;

public class SetLightStateInteractorTests
{

    #region Fields

    private readonly Mock<ILightService> m_LightService = new();
    private readonly Mock<ISetLightStateOutputPort> m_OutputPort = new();

    #endregion Fields

    #region Methods

    private Task HandleAsync(SetLightStateInputPort inputPort)
        => new SetLightStateInteractor().HandleAsync(
            inputPort,
            this.m_OutputPort.Object,
            new TestServiceFactory().With(this.m_LightService.Object).Build(),
            CancellationToken.None);

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
    public async Task HandleAsync_WhenTheLightIsUnknown_PresentsNotFoundWithTheID()
    {
        this.SetupResult(LightCommandResult.LightNotFound);

        await this.HandleAsync(PowerOn("missing-bulb"));

        this.m_OutputPort.Verify(
            o => o.PresentLightNotFoundAsync("missing-bulb", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTheProviderIsUnreachable_PresentsUnavailable()
    {
        this.SetupResult(LightCommandResult.Unavailable);

        await this.HandleAsync(PowerOn());

        this.m_OutputPort.Verify(
            o => o.PresentLightsUnavailableAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ForwardsOnlyThePropertiesThatWereSet()
    {
        LightStateChange? _Change = null;
        this.m_LightService
            .Setup(s => s.SetStateAsync(It.IsAny<string>(), It.IsAny<LightStateChange>(), It.IsAny<CancellationToken>()))
            .Callback<string, LightStateChange, CancellationToken>((_, c, _) => _Change = c)
            .ReturnsAsync(LightCommandResult.Applied);

        var _InputPort = new SetLightStateInputPort(
            "d073d5",
            new PropertyChangeTracker<bool>(),
            new PropertyChangeTracker<double>(0.4d),
            new PropertyChangeTracker<double>(),
            new PropertyChangeTracker<double>(),
            new PropertyChangeTracker<int>());

        await this.HandleAsync(_InputPort);

        _Change.Should().NotBeNull();
        _Change!.Brightness.HasBeenSet.Should().BeTrue();
        _Change.Brightness.Value.Should().Be(0.4d);
        _Change.IsOn.HasBeenSet.Should().BeFalse();
        _Change.Hue.HasBeenSet.Should().BeFalse();
        _Change.Saturation.HasBeenSet.Should().BeFalse();
        _Change.Kelvin.HasBeenSet.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_AddressesTheProviderWithTheRequestedLightID()
    {
        this.SetupResult(LightCommandResult.Applied);

        await this.HandleAsync(PowerOn("d073d5aabbcc"));

        this.m_LightService.Verify(
            s => s.SetStateAsync("d073d5aabbcc", It.IsAny<LightStateChange>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion Methods

}
