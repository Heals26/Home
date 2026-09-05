using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeSteps.UpdateRecipeStep;
using Home.Domain.Entities;
using Home.WebApi.Presenters.RecipeSteps.UpdateRecipeStep;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.RecipeSteps.UpdateRecipeStep;

/// <summary>
/// Editing a step. A step has no navigation back to its recipe, so the household is reached by
/// starting at the recipes and fanning out to their steps.
/// </summary>
public class UpdateRecipeStepInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateRecipeStepPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household, params RecipeStep[] steps)
        => new()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Steps = steps,
            Url = $"https://example.test/{recipeID}"
        };

    private static RecipeStep BuildStep(long recipeStepID, string title, string content, int sequence)
        => new()
        {
            Content = content,
            RecipeStepID = recipeStepID,
            Sequence = sequence,
            Title = title
        };

    private Task HandleAsync(
        long recipeStepID,
        PropertyChangeTracker<string> content = default,
        PropertyChangeTracker<int> sequence = default,
        PropertyChangeTracker<string> title = default)
        => new UpdateRecipeStepInteractor().HandleAsync(
            new UpdateRecipeStepInputPort(recipeStepID, content, sequence, title),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RewritesTheStepAndSavesIt()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, BuildStep(150, "Prep", "Chop the onion", 1)));

        await this.HandleAsync(150, content: new("Dice the onion"), title: new("Preparation"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();

        var _Stored = this.Stored<RecipeStep>().Single();

        _ = _Stored.Content.Should().Be("Dice the onion");
        _ = _Stored.Title.Should().Be("Preparation");
    }

    [Fact]
    public async Task HandleAsync_WhenOnlyTheContentIsSent_LeavesTheTitleAndPositionAlone()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, BuildStep(150, "Prep", "Chop the onion", 3)));

        await this.HandleAsync(150, content: new("Dice the onion"));

        var _Stored = this.Stored<RecipeStep>().Single();

        _ = _Stored.Title.Should().Be("Prep");
        _ = _Stored.Sequence.Should().Be(3);
    }

    [Fact]
    public async Task HandleAsync_CanMoveTheStepWithoutRewritingIt()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, BuildStep(150, "Prep", "Chop the onion", 3)));

        await this.HandleAsync(150, sequence: new(1));

        var _Stored = this.Stored<RecipeStep>().Single();

        _ = _Stored.Sequence.Should().Be(1);
        _ = _Stored.Content.Should().Be("Chop the onion");
    }

    [Fact]
    public async Task HandleAsync_WhenTheStepBelongsToAnotherHouseholdsRecipe_PresentsNotFoundAndChangesNothing()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, BuildStep(150, "Prep", "Ours", 1)),
            BuildRecipe(920, this.Theirs, BuildStep(950, "Secret", "Theirs", 1)));

        await this.HandleAsync(950, content: new("Rewritten by us"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<RecipeStep>().Single(s => s.RecipeStepID == 950).Content.Should().Be("Theirs");
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchStepExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, BuildStep(150, "Prep", "Chop the onion", 1)));

        await this.HandleAsync(404, content: new("Anything"));

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
