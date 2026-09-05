using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Recipes.DeleteRecipe;
using Home.Domain.Entities;
using Home.WebApi.Presenters.Recipes.DeleteRecipe;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Recipes.DeleteRecipe;

/// <summary>
/// Taking a recipe out of the book. Like deleting a card, this answers no content whatever
/// happens, so deleting twice is harmless and the response cannot be used to find out whose
/// recipes exist.
/// </summary>
public class DeleteRecipeInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteRecipePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household)
        => new()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

    private Task HandleAsync(long recipeID)
        => new DeleteRecipeInteractor().HandleAsync(
            new DeleteRecipeInputPort(recipeID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesOurRecipe()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours), BuildRecipe(121, this.Ours));

        await this.HandleAsync(120);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<Recipe>().Select(r => r.RecipeID).Should().Equal([121]);
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeBelongsToAnotherHousehold_KeepsItAndStillAnswersNoContent()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours), BuildRecipe(920, this.Theirs));

        await this.HandleAsync(920);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<Recipe>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchRecipeExists_AnswersNoContentSoDeletingTwiceIsHarmless()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(404);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<Recipe>().Should().ContainSingle();
    }

    #endregion Methods

}
