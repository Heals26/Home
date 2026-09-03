using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeIngredients.GetRecipeIngredient;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.RecipeIngredients.GetRecipeIngredient;
using Home.WebApi.UseCases.RecipeIngredients.GetRecipeIngredient;

namespace Home.Application.Tests.UseCases.RecipeIngredients.GetRecipeIngredient;

/// <summary>
/// One ingredient, with the household notes attached to it. An ingredient owns no household of its
/// own — it is reached through the recipes using it — so isolation here rests on a nested Any
/// rather than a direct comparison, which is worth pinning on its own.
/// </summary>
public class GetRecipeIngredientInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetRecipeIngredientPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household, Ingredient ingredient)
    {
        var _Recipe = new Recipe()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

        _Recipe.Ingredients = [new RecipeIngredient() { Ingredient = ingredient, Recipe = _Recipe, Sequence = 1 }];

        return _Recipe;
    }

    private Task HandleAsync(long ingredientID)
        => new GetRecipeIngredientInteractor().HandleAsync(
            new GetRecipeIngredientInputPort(ingredientID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_ReadsTheNoteBehindEveryIngredientNote()
    {
        var _Ingredient = new Ingredient() { Amount = 2, IngredientID = 130, Name = "Olive oil", Unit = MeasurementUnitSE.Tablespoons.Value };

        _Ingredient.Notes =
        [
            new IngredientNote()
            {
                Ingredient = _Ingredient,
                Note = new Note() { Content = "The good one", CreatedOnUTC = new DateTime(2026, 8, 12), NoteID = 140 }
            }
        ];

        _ = this.Database.Seed(BuildRecipe(120, this.Ours, _Ingredient));

        await this.HandleAsync(130);

        var _Response = Ok<GetRecipeIngredientApiResponse>(this.m_Presenter);

        _ = _Response.Name.Should().Be("Olive oil");
        _ = _Response.Amount.Should().Be(2);
        _ = _Response.UnitAbbreviation.Should().Be("tbsp");
        _ = _Response.Notes.Select(n => n.Content).Should().Equal(
            ["The good one"],
            "the presenter reads the note behind the join, so the query has to load it");
    }

    [Fact]
    public async Task HandleAsync_ReadsASingularUnitBesideAnAmountOfOne()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, new Ingredient()
        {
            Amount = 1,
            IngredientID = 130,
            Name = "Garlic",
            Unit = MeasurementUnitSE.Cloves.Value
        }));

        await this.HandleAsync(130);

        _ = Ok<GetRecipeIngredientApiResponse>(this.m_Presenter).UnitAbbreviation.Should().Be("clove");
    }

    [Fact]
    public async Task HandleAsync_WhenTheIngredientIsOnlyInAnotherHouseholdsRecipe_PresentsNotFound()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, new Ingredient() { IngredientID = 130, Name = "Olive oil" }),
            BuildRecipe(920, this.Theirs, new Ingredient() { IngredientID = 930, Name = "Truffle oil" }));

        await this.HandleAsync(930);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchIngredientExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildRecipe(120, this.Ours, new Ingredient() { IngredientID = 130, Name = "Olive oil" }));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
