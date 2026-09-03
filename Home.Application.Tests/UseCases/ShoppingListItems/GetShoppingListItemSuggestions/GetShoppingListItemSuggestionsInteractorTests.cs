using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingListItems.GetShoppingListItemSuggestions;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.ShoppingListItems.GetShoppingListItemSuggestions;
using Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItemSuggestions;

namespace Home.Application.Tests.UseCases.ShoppingListItems.GetShoppingListItemSuggestions;

/// <summary>
/// What the household buys, offered back while the next list is being written. The suggestions
/// carry the last amount and the last price, which is what stops anyone typing them twice.
/// </summary>
public class GetShoppingListItemSuggestionsInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetShoppingListItemSuggestionsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ShoppingList BuildList(long shoppingListID, Household household, params ShoppingListItem[] items)
    {
        var _List = new ShoppingList()
        {
            Household = household,
            Name = $"List {shoppingListID}",
            ShoppingListID = shoppingListID
        };

        foreach (var _Item in items)
            _Item.ShoppingList = _List;

        _List.Items = items;

        return _List;
    }

    private Task HandleAsync()
        => new GetShoppingListItemSuggestionsInteractor().HandleAsync(
            new GetShoppingListItemSuggestionsInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_OffersWhatTheHouseholdBuysMostFirst()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours,
                new ShoppingListItem() { Name = "Milk", Sequence = 1, ShoppingListItemID = 130 },
                new ShoppingListItem() { Name = "Bread", Sequence = 2, ShoppingListItemID = 131 }),
            BuildList(121, this.Ours,
                new ShoppingListItem() { Name = "Milk", Sequence = 1, ShoppingListItemID = 132 }));

        await this.HandleAsync();

        var _Suggestions = Ok<GetShoppingListItemSuggestionsApiResponse>(this.m_Presenter).Suggestions;

        _ = _Suggestions.Select(s => s.Name).Should().Equal(["Milk", "Bread"]);
        _ = _Suggestions.Single(s => s.Name == "Milk").TimesAdded.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_CarriesBackTheAmountAndPriceFromTheLastShopNotAnAverage()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, new ShoppingListItem()
            {
                Amount = 1,
                Cost = 2.50m,
                Name = "Milk",
                Sequence = 1,
                ShoppingListItemID = 130,
                Unit = MeasurementUnitSE.Litres.Value
            }),
            BuildList(121, this.Ours, new ShoppingListItem()
            {
                Amount = 2,
                Cost = 4.80m,
                Name = "Milk",
                Sequence = 1,
                ShoppingListItemID = 132,
                Unit = MeasurementUnitSE.Litres.Value
            }));

        await this.HandleAsync();

        var _Milk = Ok<GetShoppingListItemSuggestionsApiResponse>(this.m_Presenter).Suggestions.Single();

        _ = _Milk.Amount.Should().Be(2, "the last shop is the best guess at the next one");
        _ = _Milk.Cost.Should().Be(4.80m);
        _ = _Milk.UnitAbbreviation.Should().Be("L");
    }

    [Fact]
    public async Task HandleAsync_NeverOffersWhatOnlyAnotherHouseholdBuys()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, new ShoppingListItem() { Name = "Milk", Sequence = 1, ShoppingListItemID = 130 }),
            BuildList(920, this.Theirs, new ShoppingListItem() { Name = "Caviar", Sequence = 1, ShoppingListItemID = 930 }));

        await this.HandleAsync();

        _ = Ok<GetShoppingListItemSuggestionsApiResponse>(this.m_Presenter).Suggestions
            .Select(s => s.Name).Should().Equal(["Milk"]);
    }

    #endregion Methods

}
