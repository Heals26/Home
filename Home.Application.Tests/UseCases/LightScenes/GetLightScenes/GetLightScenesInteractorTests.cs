using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.LightScenes.GetLightScenes;
using Home.Domain.Entities;
using Home.WebApi.Presenters.LightScenes.GetLightScenes;
using Home.WebApi.UseCases.LightScenes.GetLightScenes;

namespace Home.Application.Tests.UseCases.LightScenes.GetLightScenes;

/// <summary>
/// The saved looks on the Lights page. Every row says how many bulbs it covers, which is the same
/// shape of projection that the card sections got wrong — an unloaded collection counts zero and
/// every scene claims to hold no lights.
/// </summary>
public class GetLightScenesInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetLightScenesPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private LightScene BuildScene(long lightSceneID, Household household, string name, int sequence, int lightCount, bool isPreviousLook = false)
    {
        var _Scene = new LightScene()
        {
            Household = household,
            IsPreviousLook = isPreviousLook,
            LightSceneID = lightSceneID,
            Name = name,
            Sequence = sequence
        };

        _Scene.States =
        [
            .. Enumerable.Range(1, lightCount).Select(i => new LightSceneState()
            {
                Brightness = 1,
                IsOn = true,
                Kelvin = 3500,
                Light = this.BuildLight(lightSceneID + i, household),
                LightSceneStateID = lightSceneID + i,
                Scene = _Scene
            })
        ];

        return _Scene;
    }

    private Light BuildLight(long lightID, Household household)
        => new()
        {
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
            LightID = lightID,
            Name = $"Bulb {lightID}"
        };

    private Task HandleAsync()
        => new GetLightScenesInteractor().HandleAsync(
            new GetLightScenesInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_CountsTheLightsEachSceneCovers()
    {
        _ = this.Database.Seed(
            this.BuildScene(120, this.Ours, "Movie night", 1, lightCount: 3),
            this.BuildScene(140, this.Ours, "Bedtime", 2, lightCount: 1));

        await this.HandleAsync();

        var _Scenes = Ok<GetLightScenesApiResponse>(this.m_Presenter).Scenes;

        _ = _Scenes.Single(s => s.Name == "Movie night").LightCount.Should().Be(
            3,
            "an unloaded state collection counts zero and every scene claims to hold no lights");
        _ = _Scenes.Single(s => s.Name == "Bedtime").LightCount.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_LeadsWithThePreviousLookBecauseItIsTheUndo()
    {
        _ = this.Database.Seed(
            this.BuildScene(120, this.Ours, "Movie night", 1, lightCount: 1),
            this.BuildScene(140, this.Ours, "Before", 9, lightCount: 1, isPreviousLook: true),
            this.BuildScene(160, this.Ours, "Bedtime", 2, lightCount: 1));

        await this.HandleAsync();

        _ = Ok<GetLightScenesApiResponse>(this.m_Presenter).Scenes
            .Select(s => s.Name).Should().Equal(
                ["Before", "Movie night", "Bedtime"],
                "an undo you have to hunt for is not an undo");
    }

    [Fact]
    public async Task HandleAsync_ReturnsOnlyOurHouseholdsScenes()
    {
        _ = this.Database.Seed(
            this.BuildScene(120, this.Ours, "Movie night", 1, lightCount: 1),
            this.BuildScene(920, this.Theirs, "Their look", 1, lightCount: 1));

        await this.HandleAsync();

        _ = Ok<GetLightScenesApiResponse>(this.m_Presenter).Scenes
            .Select(s => s.Name).Should().Equal(["Movie night"]);
    }

    #endregion Methods

}
