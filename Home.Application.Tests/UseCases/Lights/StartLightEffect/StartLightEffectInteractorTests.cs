using FluentAssertions;
using Home.Application.Services.Lights;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Lights.StartLightEffect;
using Home.Domain.Entities;
using Moq;

namespace Home.Application.Tests.UseCases.Lights.StartLightEffect;

public class StartLightEffectInteractorTests
{

    #region Fields

    private readonly Mock<ILightService> m_LightService = new();
    private readonly Mock<IPersistenceContext> m_PersistenceContext = new();
    private readonly Mock<IAuthorisationService> m_AuthorisationService = new();
    private readonly Mock<IStartLightEffectOutputPort> m_OutputPort = new();
    private readonly Household m_Household = new() { HouseholdID = 42 };
    private readonly LightGroup m_Group;

    #endregion Fields

    #region Constructors

    public StartLightEffectInteractorTests()
        => this.m_Group = new LightGroup()
        {
            LightGroupID = 1,
            Name = "Living Room",
            Lights = [],
            Location = new LightLocation() { Name = "Home", Household = this.m_Household }
        };

    #endregion Constructors

    #region Methods

    private Light AddLight(string id, bool hasColour, bool isConnected = true)
    {
        var _Light = new Light()
        {
            ID = id,
            Name = id,
            IsConnected = isConnected,
            HasColour = hasColour,
            Group = this.m_Group
        };

        this.m_Group.Lights.Add(_Light);

        return _Light;
    }

    private Task HandleAsync(LightEffectKind kind = LightEffectKind.Breathe)
    {
        _ = this.m_AuthorisationService.Setup(a => a.GetHousehold()).Returns(this.m_Household);
        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<Light>())
            .Returns(this.m_Group.Lights.AsQueryable());
        _ = this.m_PersistenceContext
            .Setup(c => c.GetEntities<LightGroup>())
            .Returns(new[] { this.m_Group }.AsQueryable());

        // Deliberately no StartEffectAsync setup here — a test that wants to capture the IDs
        // registers its own, and setting one here would silently replace it.
        return new StartLightEffectInteractor().HandleAsync(
            new StartLightEffectInputPort(1, kind, 120d, 1d, 1d, 3d),
            this.m_OutputPort.Object,
            new TestServiceFactory()
                .With(this.m_PersistenceContext.Object)
                .With(this.m_AuthorisationService.Object)
                .With(this.m_LightService.Object)
                .Build(),
            CancellationToken.None);
    }


    [Fact]
    public async Task HandleAsync_SkipsBulbsThatCannotShowColour()
    {
        _ = this.AddLight("colour-bulb", hasColour: true);
        _ = this.AddLight("white-bulb", hasColour: false);

        IReadOnlyCollection<string>? _Sent = null;
        this.m_LightService
            .Setup(s => s.StartEffectAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<LightEffectRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<string>, LightEffectRequest, CancellationToken>((ids, _, _) => _Sent = ids)
            .ReturnsAsync(LightCommandResult.Applied);

        await this.HandleAsync();

        _Sent.Should().BeEquivalentTo(["colour-bulb"], "breathe moves through colours, which a white-only bulb cannot do");
    }

    [Fact]
    public async Task HandleAsync_CancellingReachesWhiteOnlyBulbsToo()
    {
        _ = this.AddLight("colour-bulb", hasColour: true);
        _ = this.AddLight("white-bulb", hasColour: false);

        IReadOnlyCollection<string>? _Sent = null;
        this.m_LightService
            .Setup(s => s.StartEffectAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<LightEffectRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<string>, LightEffectRequest, CancellationToken>((ids, _, _) => _Sent = ids)
            .ReturnsAsync(LightCommandResult.Applied);

        await this.HandleAsync(LightEffectKind.Off);

        _Sent.Should().BeEquivalentTo(["colour-bulb", "white-bulb"]);
    }

    [Fact]
    public async Task HandleAsync_WhenNoBulbSupportsTheEffect_SucceedsWithoutCallingTheProvider()
    {
        _ = this.AddLight("white-bulb", hasColour: false);

        await this.HandleAsync();

        this.m_OutputPort.Verify(o => o.PresentEffectStartedAsync(It.IsAny<CancellationToken>()), Times.Once);

        this.m_LightService.Verify(
            s => s.StartEffectAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<LightEffectRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_SkipsOfflineBulbs()
    {
        _ = this.AddLight("online", hasColour: true);
        _ = this.AddLight("offline", hasColour: true, isConnected: false);

        IReadOnlyCollection<string>? _Sent = null;
        this.m_LightService
            .Setup(s => s.StartEffectAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<LightEffectRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<string>, LightEffectRequest, CancellationToken>((ids, _, _) => _Sent = ids)
            .ReturnsAsync(LightCommandResult.Applied);

        await this.HandleAsync();

        _Sent.Should().BeEquivalentTo(["online"]);
    }

    #endregion Methods

}
