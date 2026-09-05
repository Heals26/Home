using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeSteps.RemoveRecipeStep;
using Home.Domain.Entities;
using Home.WebApi.Presenters.RecipeSteps.RemoveRecipeStep;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.RecipeSteps.RemoveRecipeStep;

/// <summary>
/// Taking a step out of a recipe.
/// </summary>
public class RemoveRecipeStepInteractorTests : InteractorTest
{

    #region Fields

    private readonly RemoveRecipeStepPresenter m_Presenter = new(Mapper);

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

    private static RecipeStep BuildStep(long recipeStepID, int sequence)
        => new()
        {
            Content = $"Step {recipeStepID}",
            RecipeStepID = recipeStepID,
            Sequence = sequence,
            Title = $"Step {recipeStepID}"
        };

    private Task HandleAsync(long recipeStepID)
        => new RemoveRecipeStepInteractor().HandleAsync(
            new RemoveRecipeStepInputPort(recipeStepID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesOnlyThatStep()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, BuildStep(150, 1), BuildStep(151, 2)));

        await this.HandleAsync(150);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<RecipeStep>().Select(s => s.RecipeStepID).Should().Equal([151]);
        _ = this.Stored<Recipe>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenTheStepBelongsToAnotherHouseholdsRecipe_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, BuildStep(150, 1)),
            BuildRecipe(920, this.Theirs, BuildStep(950, 1)));

        await this.HandleAsync(950);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<RecipeStep>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchStepExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, BuildStep(150, 1)));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
