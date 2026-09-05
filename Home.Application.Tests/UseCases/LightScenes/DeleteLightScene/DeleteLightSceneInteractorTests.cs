using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.LightScenes.DeleteLightScene;
using Home.Domain.Entities;
using Home.WebApi.Presenters.LightScenes.DeleteLightScene;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.LightScenes.DeleteLightScene;

/// <summary>
/// Removing a saved look.
/// </summary>
public class DeleteLightSceneInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteLightScenePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static LightScene BuildScene(long lightSceneID, Household household, string name)
        => new()
        {
            Household = household,
            LightSceneID = lightSceneID,
            Name = name,
            Sequence = 1,
            States = []
        };

    private Task HandleAsync(long lightSceneID)
        => new DeleteLightSceneInteractor().HandleAsync(
            new DeleteLightSceneInputPort(lightSceneID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesOnlyThatScene()
    {
        _ = this.Database.Seed(BuildScene(120, this.Ours, "Movie night"), BuildScene(121, this.Ours, "Bedtime"));

        await this.HandleAsync(120);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<LightScene>().Select(s => s.Name).Should().Equal(["Bedtime"]);
    }

    [Fact]
    public async Task HandleAsync_WhenTheSceneBelongsToAnotherHousehold_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(BuildScene(120, this.Ours, "Movie night"), BuildScene(920, this.Theirs, "Theirs"));

        await this.HandleAsync(920);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<LightScene>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchSceneExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildScene(120, this.Ours, "Movie night"));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
