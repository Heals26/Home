using FluentAssertions;
using Home.Application.Services.EntityLogic.Lights;
using Home.Application.Services.Lights;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.LightScenes.ApplyLightScene;
using Home.Domain.Entities;
using Home.WebApi.Presenters.LightScenes.ApplyLightScene;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.LightScenes.ApplyLightScene;

/// <summary>
/// Turning a saved look on, and remembering what the room looked like first.
/// <para>
/// The snapshot is taken before the apply and covers the whole household rather than the lights
/// the scene touches, because "how it looked before" means the room. Taking it first is also what
/// makes tapping the previous look twice toggle back and forth.
/// </para>
/// </summary>
public class ApplyLightSceneInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<ILightSceneLogic> m_LightSceneLogic = new();
    private readonly ApplyLightScenePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Light BuildLight(long lightID, Household household, bool isOn, double brightness)
        => new()
        {
            Brightness = brightness,
            Group = new LightGroup()
            {
                LightGroupID = lightID + 100,
                Location = new LightLocation()
                {
                    Household = household,
                    ID = $"location-{lightID}",
                    LightLocationID = lightID + 200,
                    Name = "Home"
                },
                Name = "Living room",
                Sequence = 1
            },
            ID = $"bulb-{lightID}",
            IsOn = isOn,
            Kelvin = 3500,
            LightID = lightID,
            Name = $"Bulb {lightID}"
        };

    private static LightScene BuildScene(long lightSceneID, Household household, string name, bool isPreviousLook = false)
        => new()
        {
            Household = household,
            IsPreviousLook = isPreviousLook,
            LightSceneID = lightSceneID,
            Name = name,
            Sequence = 1,
            States = []
        };

    private Task HandleAsync(long lightSceneID, LightCommandResult result = LightCommandResult.Applied)
    {
        _ = this.m_LightSceneLogic
            .Setup(l => l.ApplyAsync(It.IsAny<LightScene>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        return new ApplyLightSceneInteractor().HandleAsync(
            new ApplyLightSceneInputPort(lightSceneID),
            this.m_Presenter,
            this.Services().With(this.m_LightSceneLogic.Object).Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_AppliesTheSceneAndSavesThePreviousLook()
    {
        _ = this.Database.Seed(BuildScene(120, this.Ours, "Movie night"), BuildLight(170, this.Ours, isOn: true, brightness: 0.9));

        await this.HandleAsync(120);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        this.m_LightSceneLogic.Verify(l => l.ApplyAsync(It.IsAny<LightScene>(), It.IsAny<CancellationToken>()), Times.Once);

        var _Previous = this.Stored<LightScene>().Single(s => s.IsPreviousLook);

        _ = _Previous.Name.Should().Be("Previous look");
        _ = this.Stored<LightSceneState>().Count(s => s.Scene.LightSceneID == _Previous.LightSceneID).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_SnapshotsTheWholeHouseholdNotJustTheLightsTheSceneTouches()
    {
        _ = this.Database.Seed(
            BuildScene(120, this.Ours, "Movie night"),
            BuildLight(170, this.Ours, true, 0.9),
            BuildLight(171, this.Ours, false, 0.1));

        await this.HandleAsync(120);

        var _Previous = this.Stored<LightScene>().Single(s => s.IsPreviousLook);

        _ = this.Stored<LightSceneState>().Count(s => s.Scene.LightSceneID == _Previous.LightSceneID).Should().Be(
            2,
            "how it looked before means the room, not the subset that changed");
    }

    [Fact]
    public async Task HandleAsync_ReplacesTheOldPreviousLookRatherThanPilingUp()
    {
        var _Previous = BuildScene(121, this.Ours, "Previous look", isPreviousLook: true);
        var _Light = BuildLight(170, this.Ours, true, 0.9);

        _Previous.States = [new LightSceneState() { Brightness = 0.1, Light = _Light, LightSceneStateID = 180, Scene = _Previous }];

        _ = this.Database.Seed(BuildScene(120, this.Ours, "Movie night"), _Previous, _Light);

        await this.HandleAsync(120);

        _ = this.Stored<LightScene>().Count(s => s.IsPreviousLook).Should().Be(1);
        _ = this.Stored<LightSceneState>().Count(s => s.Scene.LightSceneID == 121).Should().Be(1);
        _ = this.Stored<LightSceneState>().Single(s => s.Scene.LightSceneID == 121).Brightness.Should().Be(
            0.9,
            "the snapshot is of the room now, not whatever the last one held");
    }

    [Fact]
    public async Task HandleAsync_WhenTheLightsCannotBeReached_SavesNoPreviousLook()
    {
        _ = this.Database.Seed(BuildScene(120, this.Ours, "Movie night"), BuildLight(170, this.Ours, true, 0.9));

        await this.HandleAsync(120, LightCommandResult.Unavailable);

        _ = this.Stored<LightScene>().Should().ContainSingle(
            "nothing changed in the room, so there is no previous look to remember");
    }

    [Fact]
    public async Task HandleAsync_WhenTheSceneBelongsToAnotherHousehold_PresentsNotFoundAndAppliesNothing()
    {
        _ = this.Database.Seed(BuildScene(920, this.Theirs, "Theirs"));

        await this.HandleAsync(920);

        ShouldBeNotFound(this.m_Presenter);
        this.m_LightSceneLogic.Verify(l => l.ApplyAsync(It.IsAny<LightScene>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion Methods

}
