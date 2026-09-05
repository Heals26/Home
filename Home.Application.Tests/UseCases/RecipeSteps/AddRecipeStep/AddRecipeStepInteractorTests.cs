using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeSteps.AddRecipeStep;
using Home.Domain.Entities;
using Home.WebApi.Presenters.RecipeSteps.AddRecipeStep;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.RecipeSteps.AddRecipeStep;

/// <summary>
/// Adding a step to a recipe. Unlike most of the board, the caller chooses the position, so the
/// existing steps are projected only to attach the new one to the right recipe.
/// </summary>
public class AddRecipeStepInteractorTests : InteractorTest
{

    #region Fields

    private readonly AddRecipeStepPresenter m_Presenter = new(Mapper);

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

    private static RecipeStep BuildStep(long recipeStepID, string title, int sequence)
        => new()
        {
            Content = $"Do {title}",
            RecipeStepID = recipeStepID,
            Sequence = sequence,
            Title = title
        };

    private Task HandleAsync(long recipeID, string title, string content, int sequence)
        => new AddRecipeStepInteractor().HandleAsync(
            new AddRecipeStepInputPort(content, recipeID, sequence, title),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_AddsTheStepToTheRecipe()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(120, "Prep", "Chop the onion", 1);

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Stored = this.Stored<RecipeStep>().Single();

        _ = _Stored.Title.Should().Be("Prep");
        _ = _Stored.Content.Should().Be("Chop the onion");
        _ = _Stored.Sequence.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_LeavesTheStepsAlreadyThereAlone()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, BuildStep(150, "Prep", 1)));

        await this.HandleAsync(120, "Cook", "Fry until soft", 2);

        _ = this.Stored<RecipeStep>().Select(s => s.Title).Should().BeEquivalentTo(["Prep", "Cook"]);
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeBelongsToAnotherHousehold_PresentsNotFoundAndAddsNothing()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours), BuildRecipe(920, this.Theirs));

        await this.HandleAsync(920, "Prep", "Written by us", 1);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<RecipeStep>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchRecipeExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(404, "Prep", "Chop the onion", 1);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
