using FluentAssertions;
using Home.WebUI.Components.Pages.ShoppingList;
using Home.WebUI.DataAccess.ShoppingLists.Models;

namespace Home.Application.Tests.Components.ShoppingList;

/// <summary>
/// Adding to a list is the most-used action in the app and it goes through one text box, so every
/// way a person might write a line is pinned down here.
/// </summary>
public class ShoppingListItemLogicTests
{

    #region Methods

    [Theory]
    [InlineData("2 kg potatoes", 2, "Potatoes", 3L)]
    [InlineData("2kg potatoes", 2, "Potatoes", 3L)]
    [InlineData("500g mince", 500, "Mince", 2L)]
    [InlineData("1.5 L milk", 1.5, "Milk", 5L)]
    [InlineData("1/2 cup rice", 0.5, "Rice", 8L)]
    [InlineData("2 tins tomatoes", 2, "Tomatoes", 13L)]
    [InlineData("3 packets chips", 3, "Chips", 14L)]
    [InlineData("2 TBSP olive oil", 2, "Olive oil", 7L)]
    public void Parse_ReadsAnAmountAndItsUnit(string text, double amount, string name, long unit)
    {
        var _Parsed = ShoppingListItemLogic.Parse(text);

        _Parsed.Amount.Should().Be((decimal)amount);
        _Parsed.Name.Should().Be(name);
        _Parsed.Unit.Should().Be(unit);
    }

    [Theory]
    [InlineData("3 milk", 3, "Milk")]
    [InlineData("2 x eggs", 2, "Eggs")]
    [InlineData("6 bananas", 6, "Bananas")]
    public void Parse_ReadsACountAsAnAmountWithNoUnit(string text, double amount, string name)
    {
        var _Parsed = ShoppingListItemLogic.Parse(text);

        _Parsed.Amount.Should().Be((decimal)amount);
        _Parsed.Name.Should().Be(name);
        _Parsed.Unit.Should().BeNull();
    }

    [Theory]
    [InlineData("milk", "Milk")]
    [InlineData("BBQ sauce", "BBQ sauce")]
    [InlineData("  pak   choi  ", "Pak choi")]
    // A number welded to letters is a brand, not an amount, and an amount with nothing after it
    // is not an amount at all — both have to survive as the name exactly as typed.
    [InlineData("7up", "7up")]
    [InlineData("500", "500")]
    [InlineData("2 kg", "2 kg")]
    public void Parse_LeavesTextItCannotReadAsTheName(string text, string name)
    {
        var _Parsed = ShoppingListItemLogic.Parse(text);

        _Parsed.Amount.Should().BeNull();
        _Parsed.Name.Should().Be(name);
        _Parsed.Unit.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_ReturnsNothingForEmptyText(string? text)
    {
        var _Parsed = ShoppingListItemLogic.Parse(text);

        _Parsed.Amount.Should().BeNull();
        _Parsed.Name.Should().BeEmpty();
        _Parsed.Unit.Should().BeNull();
    }

    [Fact]
    public void DescribeAmount_ReadsTheAmountWithItsUnit()
        => ShoppingListItemLogic.DescribeAmount(new ShoppingListItemDto() { Amount = 2, Unit = 3, UnitAbbreviation = "kg" })
            .Should().Be("2 kg");

    [Fact]
    public void DescribeAmount_ReadsAnAmountWithNoUnitAsABareNumber()
        => ShoppingListItemLogic.DescribeAmount(new ShoppingListItemDto() { Amount = 3 })
            .Should().Be("3");

    [Fact]
    public void DescribeAmount_ResolvesTheUnitWhenTheApiSentNoAbbreviation()
        => ShoppingListItemLogic.DescribeAmount(new ShoppingListItemDto() { Amount = 500, Unit = 2 })
            .Should().Be("500 g");

    [Fact]
    public void DescribeAmount_FallsBackToTheColumnsUsedBeforeUnitsExisted()
        => ShoppingListItemLogic.DescribeAmount(new ShoppingListItemDto() { Quantity = 2, Weight = 500 })
            .Should().Be("2, 500 g");

    [Fact]
    public void DescribeAmount_SaysNothingForAnItemWithNoAmount()
        => ShoppingListItemLogic.DescribeAmount(new ShoppingListItemDto()).Should().BeEmpty();

    #endregion Methods

}
