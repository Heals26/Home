using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeImages.SetRecipeImage;
using Home.Domain.Entities;
using Home.WebApi.Presenters.RecipeImages.SetRecipeImage;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.RecipeImages.SetRecipeImage;

/// <summary>
/// Storing the household's photo of a dish. It replaces whatever was there rather than piling up,
/// and the content type is worked out from the bytes rather than trusted from the upload.
/// </summary>
public class SetRecipeImageInteractorTests : InteractorTest
{

    #region Constants

    /// <summary>The first bytes of a PNG, which is what the type detection keys off.</summary>
    private static readonly byte[] s_Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    #endregion Constants

    #region Fields

    private readonly SetRecipeImagePresenter m_Presenter = new(Mapper);

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

    private Task HandleAsync(long recipeID, byte[] content)
        => new SetRecipeImageInteractor().HandleAsync(
            new SetRecipeImageInputPort(content, recipeID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_StoresTheBytesAndStampsTheRecipe()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(120, s_Png);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();

        var _Stored = this.Stored<RecipeImage>().Single();

        _ = _Stored.Content.Should().Equal(s_Png);
        _ = _Stored.ContentType.Should().Be("image/png", "the type is read out of the bytes, not taken from the upload");
        _ = this.Stored<Recipe>().Single().ImageUpdatedOnUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
    }

    [Fact]
    public async Task HandleAsync_ReplacesAPhotoRatherThanAddingASecond()
    {
        var _Recipe = BuildRecipe(120, this.Ours);

        _ = this.Database.Seed(new RecipeImage()
        {
            Content = [0x01],
            ContentType = "image/jpeg",
            Recipe = _Recipe,
            RecipeImageID = 1120
        });

        await this.HandleAsync(120, s_Png);

        _ = this.Stored<RecipeImage>().Should().ContainSingle();
        _ = this.Stored<RecipeImage>().Single().Content.Should().Equal(s_Png);
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeBelongsToAnotherHousehold_PresentsNotFoundAndStoresNothing()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours), BuildRecipe(920, this.Theirs));

        await this.HandleAsync(920, s_Png);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<RecipeImage>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchRecipeExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(404, s_Png);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
