using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Recipes.UpdateRecipe;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.Recipes.UpdateRecipe;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Recipes.UpdateRecipe;

/// <summary>
/// Editing a recipe's own details. Eight properties, all through change trackers, and one of them
/// treats an empty string as "take the picture off" rather than storing a blank.
/// </summary>
public class UpdateRecipeInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateRecipePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household)
        => new()
        {
            Complexity = RecipeComplexitySE.Easy.Value,
            CookMinutes = 30,
            Household = household,
            ImageUrl = "https://example.test/photo.jpg",
            Name = $"Recipe {recipeID}",
            PrepMinutes = 10,
            RecipeID = recipeID,
            Servings = 4,
            Url = $"https://example.test/{recipeID}"
        };

    private Task HandleAsync(
        long recipeID,
        PropertyChangeTracker<long?> complexity = default,
        PropertyChangeTracker<int?> cookMinutes = default,
        PropertyChangeTracker<string> imageUrl = default,
        PropertyChangeTracker<string> name = default,
        PropertyChangeTracker<int?> prepMinutes = default,
        PropertyChangeTracker<int?> servings = default,
        PropertyChangeTracker<string> url = default)
        => new UpdateRecipeInteractor().HandleAsync(
            new UpdateRecipeInputPort(complexity, cookMinutes, imageUrl, name, prepMinutes, recipeID, servings, url),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RenamesTheRecipeAndSavesIt()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(120, name: new("Spaghetti Bolognese"));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<Recipe>().Single().Name.Should().Be("Spaghetti Bolognese");
    }

    [Fact]
    public async Task HandleAsync_LeavesEveryPropertyNobodySent()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(120, name: new("Renamed"));

        var _Stored = this.Stored<Recipe>().Single();

        _ = _Stored.CookMinutes.Should().Be(30);
        _ = _Stored.PrepMinutes.Should().Be(10);
        _ = _Stored.Servings.Should().Be(4);
        _ = _Stored.ImageUrl.Should().Be("https://example.test/photo.jpg");
    }

    [Fact]
    public async Task HandleAsync_TreatsAnEmptyPictureAddressAsTakingThePictureOff()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(120, imageUrl: new("   "));

        _ = this.Stored<Recipe>().Single().ImageUrl.Should().BeNull(
            "a blank address is nobody asking for a blank picture, it is asking for none");
    }

    [Fact]
    public async Task HandleAsync_TrimsAPictureAddress()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(120, imageUrl: new("  https://example.test/new.jpg  "));

        _ = this.Stored<Recipe>().Single().ImageUrl.Should().Be("https://example.test/new.jpg");
    }

    [Fact]
    public async Task HandleAsync_CanClearTheTimesAndServingsForARecipeNobodyHasJudged()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours));

        await this.HandleAsync(120, complexity: new(null), cookMinutes: new(null), prepMinutes: new(null), servings: new(null));

        var _Stored = this.Stored<Recipe>().Single();

        _ = _Stored.Complexity.Should().BeNull();
        _ = _Stored.CookMinutes.Should().BeNull();
        _ = _Stored.PrepMinutes.Should().BeNull();
        _ = _Stored.Servings.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenTheRecipeBelongsToAnotherHousehold_PresentsNotFoundAndChangesNothing()
    {
        _ = this.Database.Seed(BuildRecipe(920, this.Theirs));

        await this.HandleAsync(920, name: new("Renamed by us"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Recipe>().Single().Name.Should().Be("Recipe 920");
    }

    #endregion Methods

}
