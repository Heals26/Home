using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.LightScenes.CaptureLightScene;
using Home.Domain.Entities;
using Home.WebApi.Presenters.LightScenes.CaptureLightScene;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.LightScenes.CaptureLightScene;

/// <summary>
/// Saving how the lights look right now. It reads the cached rows rather than calling the
/// provider, so capturing costs nothing and is only as fresh as the last sync.
/// </summary>
public class CaptureLightSceneInteractorTests : InteractorTest
{

    #region Fields

    private readonly CaptureLightScenePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static LightGroup BuildGroup(long lightGroupID, Household household, string name)
        => new()
        {
            LightGroupID = lightGroupID,
            Location = new LightLocation()
            {
                Household = household,
                ID = $"location-{lightGroupID}",
                LightLocationID = lightGroupID + 100,
                Name = "Home"
            },
            Name = name,
            Sequence = 1
        };

    private static Light BuildLight(long lightID, LightGroup group, bool isOn, double brightness)
        => new()
        {
            Brightness = brightness,
            Group = group,
            Hue = 120,
            ID = $"bulb-{lightID}",
            IsOn = isOn,
            Kelvin = 3500,
            LightID = lightID,
            Name = $"Bulb {lightID}",
            Saturation = 0.5
        };

    private Task HandleAsync(string name, long? lightGroupID = null)
        => new CaptureLightSceneInteractor().HandleAsync(
            new CaptureLightSceneInputPort(name, lightGroupID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_SavesTheStateOfEveryLightInTheHousehold()
    {
        var _Group = BuildGroup(160, this.Ours, "Living room");

        _ = this.Database.Seed(BuildLight(170, _Group, isOn: true, brightness: 0.8), BuildLight(171, _Group, isOn: false, brightness: 0.2));

        await this.HandleAsync("Movie night");

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();
        _ = this.Stored<LightScene>().Single().Name.Should().Be("Movie night");
        _ = this.Stored<LightSceneState>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_CopiesEachLightsCachedStateRatherThanAskingTheProvider()
    {
        var _Group = BuildGroup(160, this.Ours, "Living room");

        _ = this.Database.Seed(BuildLight(170, _Group, isOn: true, brightness: 0.8));

        await this.HandleAsync("Movie night");

        var _State = this.Stored<LightSceneState>().Single();

        _ = _State.IsOn.Should().BeTrue();
        _ = _State.Brightness.Should().Be(0.8);
        _ = _State.Kelvin.Should().Be(3500);
    }

    [Fact]
    public async Task HandleAsync_WhenAGroupIsNamed_CapturesOnlyThatGroup()
    {
        var _Living = BuildGroup(160, this.Ours, "Living room");
        var _Bedroom = BuildGroup(161, this.Ours, "Bedroom");

        _ = this.Database.Seed(BuildLight(170, _Living, true, 0.8), BuildLight(171, _Bedroom, true, 0.4));

        await this.HandleAsync("Living room evening", lightGroupID: 160);

        _ = this.Stored<LightSceneState>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_PutsANewSceneOnTheEnd()
    {
        var _Group = BuildGroup(160, this.Ours, "Living room");

        _ = this.Database.Seed(
            new LightScene() { Household = this.Ours, LightSceneID = 120, Name = "Bedtime", Sequence = 4, States = [] },
            BuildLight(170, _Group, true, 0.8));

        await this.HandleAsync("Movie night");

        _ = this.Stored<LightScene>().Single(s => s.Name == "Movie night").Sequence.Should().Be(5);
    }

    [Fact]
    public async Task HandleAsync_WhenThereAreNoLightsToCapture_RefusesRatherThanSavingAnEmptyScene()
    {
        _ = this.Database.Seed(this.Ours);

        await this.HandleAsync("Movie night");

        _ = this.Stored<LightScene>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_NeverCapturesAnotherHouseholdsLights()
    {
        var _Theirs = BuildGroup(960, this.Theirs, "Their room");

        _ = this.Database.Seed(BuildLight(970, _Theirs, true, 0.8));

        await this.HandleAsync("Movie night");

        _ = this.Stored<LightSceneState>().Should().BeEmpty();
    }

    #endregion Methods

}
