using Home.Application.Services.Lights;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Lights.GetLights;
using Moq;

namespace Home.Application.Tests.UseCases.Lights.GetLights;

public class GetLightsInteractorTests
{

    #region Fields

    private readonly Mock<ILightService> m_LightService = new();
    private readonly Mock<IGetLightsOutputPort> m_OutputPort = new();

    #endregion Fields

    #region Methods

    private static LightSnapshot BuildLight(string id, string label = "Bedside")
        => new(id, label, "g1", "Bedroom", "Home", true, true, 0.8d, 120d, 1d, 3500);

    private Task HandleAsync()
        => new GetLightsInteractor().HandleAsync(
            new GetLightsInputPort(),
            this.m_OutputPort.Object,
            new TestServiceFactory().With(this.m_LightService.Object).Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WhenTheProviderReturnsLights_PresentsThem()
    {
        var _Lights = new[] { BuildLight("d073d5"), BuildLight("d073d6", "Desk") };

        _ = this.m_LightService
            .Setup(s => s.GetLightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_Lights);

        await this.HandleAsync();

        this.m_OutputPort.Verify(
            o => o.PresentLightsAsync(
                It.Is<IReadOnlyList<LightSnapshot>>(l => l.Count == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTheProviderIsUnreachable_PresentsUnavailable()
    {
        _ = this.m_LightService
            .Setup(s => s.GetLightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<LightSnapshot>?)null);

        await this.HandleAsync();

        this.m_OutputPort.Verify(
            o => o.PresentLightsUnavailableAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        this.m_OutputPort.Verify(
            o => o.PresentLightsAsync(It.IsAny<IReadOnlyList<LightSnapshot>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTheAccountHasNoLights_PresentsAnEmptyListRatherThanUnavailable()
    {
        _ = this.m_LightService
            .Setup(s => s.GetLightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<LightSnapshot>());

        await this.HandleAsync();

        this.m_OutputPort.Verify(
            o => o.PresentLightsAsync(
                It.Is<IReadOnlyList<LightSnapshot>>(l => l.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);

        this.m_OutputPort.Verify(
            o => o.PresentLightsUnavailableAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion Methods

}
