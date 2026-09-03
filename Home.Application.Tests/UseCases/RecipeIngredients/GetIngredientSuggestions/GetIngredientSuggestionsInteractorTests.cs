using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.RecipeIngredients.GetIngredientSuggestions;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.RecipeIngredients.GetIngredientSuggestions;
using Home.WebApi.UseCases.RecipeIngredients.GetIngredientSuggestions;

namespace Home.Application.Tests.UseCases.RecipeIngredients.GetIngredientSuggestions;

/// <summary>
/// What the household has cooked with before, offered while an ingredient is being typed. Added on
/// 31 Aug without tests, and it is the read most likely to leak: an ingredient row carries no
/// household, only the recipes reaching it.
/// </summary>
public class GetIngredientSuggestionsInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetIngredientSuggestionsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static Recipe BuildRecipe(long recipeID, Household household, params Ingredient[] ingredients)
    {
        var _Recipe = new Recipe()
        {
            Household = household,
            Name = $"Recipe {recipeID}",
            RecipeID = recipeID,
            Url = $"https://example.test/{recipeID}"
        };

        _Recipe.Ingredients = [.. ingredients.Select((i, index) => new RecipeIngredient() { Ingredient = i, Recipe = _Recipe, Sequence = index })];

        return _Recipe;
    }

    private Task HandleAsync()
        => new GetIngredientSuggestionsInteractor().HandleAsync(
            new GetIngredientSuggestionsInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_OffersTheMostUsedIngredientsFirst()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours,
                new Ingredient() { IngredientID = 130, Name = "Onion" },
                new Ingredient() { IngredientID = 131, Name = "Garlic" }),
            BuildRecipe(121, this.Ours,
                new Ingredient() { IngredientID = 132, Name = "Onion" }));

        await this.HandleAsync();

        var _Suggestions = Ok<GetIngredientSuggestionsApiResponse>(this.m_Presenter).Suggestions;

        _ = _Suggestions.Select(s => s.Name).Should().Equal(["Onion", "Garlic"]);
        _ = _Suggestions.Single(s => s.Name == "Onion").TimesUsed.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_SuggestsTheAmountFromTheLastTimeItWasWrittenNotAnAverage()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, new Ingredient() { Amount = 1, IngredientID = 130, Name = "Onion", Unit = MeasurementUnitSE.Pieces.Value }),
            BuildRecipe(121, this.Ours, new Ingredient() { Amount = 500, IngredientID = 132, Name = "Onion", Unit = MeasurementUnitSE.Grams.Value }));

        await this.HandleAsync();

        var _Onion = Ok<GetIngredientSuggestionsApiResponse>(this.m_Presenter).Suggestions.Single();

        _ = _Onion.Amount.Should().Be(500, "the last recipe to use it is the best guess at the next one");
        _ = _Onion.UnitAbbreviation.Should().Be("g");
    }

    [Fact]
    public async Task HandleAsync_NeverOffersAnIngredientOnlyAnotherHouseholdHasCookedWith()
    {
        _ = this.Database.Seed(
            BuildRecipe(120, this.Ours, new Ingredient() { IngredientID = 130, Name = "Onion" }),
            BuildRecipe(920, this.Theirs, new Ingredient() { IngredientID = 930, Name = "Truffle" }));

        await this.HandleAsync();

        _ = Ok<GetIngredientSuggestionsApiResponse>(this.m_Presenter).Suggestions
            .Select(s => s.Name).Should().Equal(
                ["Onion"],
                "an ingredient carries no household, so only the recipes reaching it keep the larders apart");
    }

    [Fact]
    public async Task HandleAsync_WhenTheHouseholdHasCookedNothing_OffersNothing()
    {
        _ = this.Database.Seed(BuildRecipe(920, this.Theirs, new Ingredient() { IngredientID = 930, Name = "Truffle" }));

        await this.HandleAsync();

        _ = Ok<GetIngredientSuggestionsApiResponse>(this.m_Presenter).Suggestions.Should().BeEmpty();
    }

    #endregion Methods

}
