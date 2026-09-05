using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.LightScenes.SetLightSceneSequence;
using Home.Domain.Entities;
using Home.WebApi.Presenters.LightScenes.SetLightSceneSequence;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.LightScenes.SetLightSceneSequence;

/// <summary>
/// Moving a scene up or down the Lights page. Half of the two-call swap the reorder control makes.
/// </summary>
public class SetLightSceneSequenceInteractorTests : InteractorTest
{

    #region Fields

    private readonly SetLightSceneSequencePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static LightScene BuildScene(long lightSceneID, Household household, string name, int sequence)
        => new()
        {
            Household = household,
            LightSceneID = lightSceneID,
            Name = name,
            Sequence = sequence,
            States = []
        };

    private Task HandleAsync(long lightSceneID, int sequence)
        => new SetLightSceneSequenceInteractor().HandleAsync(
            new SetLightSceneSequenceInputPort(lightSceneID, sequence),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_MovesTheSceneAndSavesIt()
    {
        _ = this.Database.Seed(BuildScene(120, this.Ours, "Movie night", 1), BuildScene(121, this.Ours, "Bedtime", 2));

        await this.HandleAsync(121, 0);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<LightScene>().Single(s => s.LightSceneID == 121).Sequence.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_LeavesTheOtherScenesWhereTheyAre()
    {
        _ = this.Database.Seed(BuildScene(120, this.Ours, "Movie night", 1), BuildScene(121, this.Ours, "Bedtime", 2));

        await this.HandleAsync(121, 0);

        _ = this.Stored<LightScene>().Single(s => s.LightSceneID == 120).Sequence.Should().Be(
            1,
            "the caller makes the swap in two calls, so this one moves exactly what it was asked to");
    }

    [Fact]
    public async Task HandleAsync_WhenTheSceneBelongsToAnotherHousehold_PresentsNotFoundAndChangesNothing()
    {
        _ = this.Database.Seed(BuildScene(120, this.Ours, "Movie night", 1), BuildScene(920, this.Theirs, "Theirs", 1));

        await this.HandleAsync(920, 9);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<LightScene>().Single(s => s.LightSceneID == 920).Sequence.Should().Be(1);
    }

    #endregion Methods

}
