using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeImages.DeleteRecipeImage;
using Home.Domain.Entities;
using Home.WebApi.Presenters.RecipeImages.DeleteRecipeImage;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.RecipeImages.DeleteRecipeImage;

/// <summary>
/// Taking the household's photo off a recipe. Two rows have to agree: the bytes go, and the
/// timestamp on the recipe that says a photo exists has to be cleared with them.
/// </summary>
public class DeleteRecipeImageInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteRecipeImagePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household, bool withPhoto)
    {
        var _Recipe = new Recipe()
        {
            Household = household,
            ImageUpdatedOnUTC = withPhoto ? new DateTime(2026, 8, 12) : null,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

        return _Recipe;
    }

    private static RecipeImage BuildPhoto(Recipe recipe)
        => new()
        {
            Content = [0x89, 0x50, 0x4E, 0x47],
            ContentType = "image/png",
            Recipe = recipe,
            RecipeImageID = recipe.RecipeID + 1000
        };

    private Task HandleAsync(long recipeID)
        => new DeleteRecipeImageInteractor().HandleAsync(
            new DeleteRecipeImageInputPort(recipeID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesTheBytesAndClearsTheTimestampTogether()
    {
        _ = this.Database.Seed(BuildPhoto(BuildRecipe(120, this.Ours, withPhoto: true)));

        await this.HandleAsync(120);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<RecipeImage>().Should().BeEmpty();
        _ = this.Stored<Recipe>().Single().ImageUpdatedOnUTC.Should().BeNull(
            "the book decides whether a recipe has a photo from this timestamp, not from the bytes");
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeHasNoPhoto_StillAnswersNoContent()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, withPhoto: false));

        await this.HandleAsync(120);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeBelongsToAnotherHousehold_KeepsThePhoto()
    {
        _ = this.Database.Seed(BuildPhoto(BuildRecipe(920, this.Theirs, withPhoto: true)));

        await this.HandleAsync(920);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<RecipeImage>().Should().ContainSingle();
        _ = this.Stored<Recipe>().Single().ImageUpdatedOnUTC.Should().NotBeNull();
    }

    #endregion Methods

}
