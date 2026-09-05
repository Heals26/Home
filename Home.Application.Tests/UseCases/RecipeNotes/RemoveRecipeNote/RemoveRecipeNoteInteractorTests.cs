using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeNotes.RemoveRecipeNote;
using Home.Domain.Entities;
using Home.WebApi.Presenters.RecipeNotes.RemoveRecipeNote;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.RecipeNotes.RemoveRecipeNote;

/// <summary>
/// Taking a note off a recipe. The note itself goes too, because a note reaches a household only
/// through the thing it is pinned to and one with no join is unreachable.
/// </summary>
public class RemoveRecipeNoteInteractorTests : InteractorTest
{

    #region Fields

    private readonly RemoveRecipeNotePresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household, params long[] noteIDs)
    {
        var _Recipe = new Recipe()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

        _Recipe.Notes =
        [
            .. noteIDs.Select(id => new RecipeNote()
            {
                Note = new Note() { Content = $"Note {id}", CreatedOnUTC = new DateTime(2026, 8, 12), NoteID = id },
                Recipe = _Recipe
            })
        ];

        return _Recipe;
    }

    private Task HandleAsync(long recipeID, long noteID)
        => new RemoveRecipeNoteInteractor().HandleAsync(
            new RemoveRecipeNoteInputPort(noteID, recipeID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesTheJoinAndTheNoteBehindIt()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, 140));

        await this.HandleAsync(120, 140);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<RecipeNote>().Should().BeEmpty();
        _ = this.Stored<Note>().Should().BeEmpty("a note with no join is unreachable, so it would only be litter");
    }

    [Fact]
    public async Task HandleAsync_LeavesTheOtherNotesAndTheRecipeAlone()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, 140, 141));

        await this.HandleAsync(120, 140);

        _ = this.Stored<Note>().Select(n => n.NoteID).Should().Equal([141]);
        _ = this.Stored<Recipe>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenTheNoteIsOnAnotherHouseholdsRecipe_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, 140),
            BuildRecipe(920, this.Theirs, 940));

        await this.HandleAsync(920, 940);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Note>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenTheNoteIsOnADifferentRecipeOfOurs_PresentsNotFound()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, 140),
            BuildRecipe(121, this.Ours, 141));

        await this.HandleAsync(120, 141);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<Note>().Should().HaveCount(2);
    }

    #endregion Methods

}
