using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeImages.GetRecipeImage;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.ObjectResults;
using Home.WebApi.Presenters.RecipeImages.GetRecipeImage;

namespace Home.Application.Tests.UseCases.RecipeImages.GetRecipeImage;

/// <summary>
/// The household's own photo of a dish. The only read that answers with bytes rather than a
/// response model, and the only one whose projection exists to keep something out — the query
/// names <c>Content</c> and <c>ContentType</c> so listing the book never drags image bytes along.
/// </summary>
public class GetRecipeImageInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetRecipeImagePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static RecipeImage BuildPhoto(long recipeID, Household household, byte[] content, string contentType)
        => new()
        {
            Content = content,
            ContentType = contentType,
            Recipe = new Recipe()
            {
                Household = household,
                ImageUpdatedOnUTC = new DateTime(2026, 8, 12),
                Name = $"Recipe {recipeID}",
                RecipeID = recipeID,
                Url = $"https://example.test/{recipeID}"
            },
            RecipeImageID = recipeID + 1000
        };

    private Task HandleAsync(long recipeID)
        => new GetRecipeImageInteractor().HandleAsync(
            new GetRecipeImageInputPort(recipeID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WhenTheRecipeIsOurs_StreamsBackTheBytesAsUploaded()
    {
        byte[] _Content = [0x89, 0x50, 0x4E, 0x47];

        _ = this.Database.Seed(BuildPhoto(120, this.Ours, _Content, "image/png"));

        await this.HandleAsync(120);

        _ = this.m_Presenter.PresentedSuccessfully.Should().BeTrue();

        var _Result = this.m_Presenter.Result.Should().BeOfType<HomeStreamResult>().Which;
        _ = _Result.ContentType.Should().Be("image/png");

        using var _Received = new MemoryStream();
        await _Result.FileStream.CopyToAsync(_Received);

        _ = _Received.ToArray().Should().Equal(_Content);
    }

    [Fact]
    public async Task HandleAsync_WhenThePhotoBelongsToAnotherHousehold_PresentsNotFound()
    {
        _ = this.Database.Seed(
            BuildPhoto(120, this.Ours, [0x89], "image/png"),
            BuildPhoto(920, this.Theirs, [0xFF], "image/jpeg"));

        await this.HandleAsync(920);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeHasNoPhoto_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildPhoto(120, this.Ours, [0x89], "image/png"));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
