using FluentAssertions;
using Home.WebUI.DataAccess.ShoppingCategories.Models;
using Home.WebUI.DataAccess.ShoppingLists.Models;
using Home.WebUI.Infrastructure.Services.ShoppingLists;
using Home.WebUI.Infrastructure.ShoppingLists;

namespace Home.Application.Tests.Components.ShoppingList;

/// <summary>
/// How a list reads when it is grouped by aisle, and what it comes to before anyone leaves.
/// </summary>
public class ShoppingAisleLogicTests
{

    #region Fields

    private static readonly ShoppingCategoryDto s_Dairy = new() { Name = "Dairy", Sequence = 1, ShoppingCategoryID = 110 };
    private static readonly ShoppingCategoryDto s_Frozen = new() { Name = "Frozen", Sequence = 2, ShoppingCategoryID = 112 };
    private static readonly ShoppingCategoryDto s_Fruit = new() { Name = "Fruit and veg", Sequence = 0, ShoppingCategoryID = 111 };

    private readonly IShoppingAisleLogic m_Logic = new ShoppingAisleLogic();

    #endregion Fields

    #region Methods

    private static ShoppingListItemDto Item(long shoppingListItemID, string name, long? aisle = null, long? sequence = null, decimal? cost = null, decimal? estimate = null)
        => new()
        {
            Cost = cost,
            EstimatedCost = estimate,
            Name = name,
            Sequence = sequence ?? shoppingListItemID,
            ShoppingCategoryID = aisle,
            ShoppingListItemID = shoppingListItemID
        };

    [Fact]
    public void Group_PutsTheAislesInTheOrderTheHouseholdWalksThem()
    {
        var _Groups = this.m_Logic.Group(
            [Item(1, "Milk", 110), Item(2, "Apples", 111), Item(3, "Peas", 112)],
            [s_Dairy, s_Frozen, s_Fruit]);

        _ = _Groups.Select(g => g.Aisle!.Name).Should().Equal("Fruit and veg", "Dairy", "Frozen");
    }

    [Fact]
    public void Group_LeavesOutAislesWithNothingOnTheList()
    {
        var _Groups = this.m_Logic.Group([Item(1, "Milk", 110)], [s_Dairy, s_Frozen, s_Fruit]);

        _ = _Groups.Should().ContainSingle().Which.Aisle.Should().Be(s_Dairy);
    }

    [Fact]
    public void Group_PutsWhatHasNoAisleLast()
    {
        var _Groups = this.m_Logic.Group([Item(1, "Bin bags"), Item(2, "Milk", 110)], [s_Dairy]);

        _ = _Groups.Should().HaveCount(2);
        _ = _Groups[0].Aisle.Should().Be(s_Dairy);
        _ = _Groups[1].Aisle.Should().BeNull();
    }

    [Fact]
    public void Group_TreatsAnAisleTheHouseholdNoLongerHasAsNone()
    {
        var _Groups = this.m_Logic.Group([Item(1, "Milk", 999)], [s_Dairy]);

        _ = _Groups.Should().ContainSingle().Which.Aisle.Should().BeNull("the aisle was removed on another phone after this list loaded");
    }

    [Fact]
    public void Group_KeepsTheListsOwnOrderInsideAnAisle()
    {
        var _Groups = this.m_Logic.Group(
            [Item(1, "Yoghurt", 110, sequence: 3), Item(2, "Milk", 110, sequence: 1), Item(3, "Butter", 110, sequence: 2)],
            [s_Dairy]);

        _ = _Groups.Single().Items.Select(i => i.Name).Should().Equal("Milk", "Butter", "Yoghurt");
    }

    [Fact]
    public void Estimate_TakesAnUnpricedLineAtWhatItCostLastTime()
    {
        var _Estimate = this.m_Logic.Estimate([Item(1, "Milk", cost: 4.00m), Item(2, "Bread", estimate: 3.50m)]);

        _ = _Estimate.Should().Be(new ShoppingListEstimate(true, 7.50m, 0));
    }

    [Fact]
    public void Estimate_CountsWhatItCannotPriceAndLeavesItOut()
    {
        var _Estimate = this.m_Logic.Estimate([Item(1, "Milk", cost: 4.00m), Item(2, "Saffron")]);

        _ = _Estimate.Should().Be(new ShoppingListEstimate(false, 4.00m, 1));
    }

    [Fact]
    public void Estimate_PrefersThePriceOnTheListToTheGuess()
        => _ = this.m_Logic.Estimate([Item(1, "Milk", cost: 5.00m, estimate: 4.00m)]).Total.Should().Be(5.00m);

    [Fact]
    public void FindNameClash_IgnoresCaseAndSpaces()
        => _ = this.m_Logic.FindNameClash([s_Dairy, s_Frozen], "  dairy ", null).Should().Be(s_Dairy);

    [Fact]
    public void FindNameClash_LetsAnAisleChangeTheCaseOfItsOwnName()
        => _ = this.m_Logic.FindNameClash([s_Dairy, s_Frozen], "DAIRY", s_Dairy.ShoppingCategoryID).Should().BeNull();

    #endregion Methods

}
