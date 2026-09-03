using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeSteps.GetRecipeStep;
using Home.Domain.Entities;
using Home.WebApi.Presenters.RecipeSteps.GetRecipeStep;
using Home.WebApi.UseCases.RecipeSteps.GetRecipeStep;

namespace Home.Application.Tests.UseCases.RecipeSteps.GetRecipeStep;

/// <summary>
/// One step of one recipe. A step has no navigation back to its recipe, so the household is
/// reached by starting at the recipe and fanning out — the one read in the application that
/// scopes by SelectMany rather than by walking upwards.
/// </summary>
public class GetRecipeStepInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetRecipeStepPresenter m_Presenter = new(Mapper);

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

    private Task HandleAsync(long recipeStepID)
        => new GetRecipeStepInteractor().HandleAsync(
            new GetRecipeStepInputPort(recipeStepID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WhenTheStepIsOurs_PresentsIt()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours,
            BuildStep(140, "Prep", "Chop the onion", 1),
            BuildStep(141, "Cook", "Fry until soft", 2)));

        await this.HandleAsync(141);

        var _Response = Ok<GetRecipeStepApiResponse>(this.m_Presenter);

        _ = _Response.RecipeStepID.Should().Be(141);
        _ = _Response.Title.Should().Be("Cook");
        _ = _Response.Content.Should().Be("Fry until soft");
        _ = _Response.Sequence.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_WhenTheStepBelongsToAnotherHouseholdsRecipe_PresentsNotFound()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, BuildStep(140, "Prep", "Chop the onion", 1)),
            BuildRecipe(920, this.Theirs, BuildStep(940, "Secret", "Their method", 1)));

        await this.HandleAsync(940);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchStepExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, BuildStep(140, "Prep", "Chop the onion", 1)));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
