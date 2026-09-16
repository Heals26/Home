using FluentAssertions;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.UseCases.ShoppingLists.Models;
using Home.Domain.Entities;
using Home.Domain.Enumerations;

namespace Home.Application.Tests.Infrastructure.ShoppingLists;

/// <summary>
/// The sums behind "usually" and "dearer than usual" (11 Sep). Prices compare per unit, so a bigger
/// pack is never flagged for costing more in total.
/// </summary>
public class ShoppingPriceLogicTests
{

    #region Methods

    private static ShoppingListItem Line(decimal? amount, MeasurementUnitSE? unit, decimal? cost, long shoppingListItemID = 1, long? tickedOnTripID = null)
        => new()
        {
            Amount = amount,
            Cost = cost,
            InBasket = tickedOnTripID != null,
            Name = "Milk",
            ShoppingListItemID = shoppingListItemID,
            ShoppingTripID = tickedOnTripID,
            Unit = unit?.Value
        };

    /// <summary>
    /// Past purchases a day apart, newest first, bought on lines numbered from 100.
    /// </summary>
    private static ShoppingItemMemory Memory(params (decimal Cost, decimal? Amount, MeasurementUnitSE? Unit)[] newestFirst)
    {
        var _Memory = new ShoppingItemMemory() { Name = "Milk", NameKey = "milk" };

        _Memory.Prices =
        [
            .. newestFirst.Select((p, index) => new ShoppingItemPrice()
            {
                Amount = p.Amount,
                BoughtOnUTC = TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-index),
                Cost = p.Cost,
                Memory = _Memory,
                ShoppingListItemID = 100 + index,
                Unit = p.Unit?.Value
            })
        ];

        return _Memory;
    }

    /// <summary>
    /// Past purchases a week apart, newest first, all made on the one line, the way a list kept from
    /// week to week with Untick all buys the same thing on the same line every time. Each week is its
    /// own trip, numbered up to the newest.
    /// </summary>
    private static ShoppingItemMemory ReusedLine(long shoppingListItemID, params decimal[] newestFirst)
    {
        var _Memory = new ShoppingItemMemory() { Name = "Milk", NameKey = "milk" };

        _Memory.Prices =
        [
            .. newestFirst.Select((cost, index) => new ShoppingItemPrice()
            {
                BoughtOnUTC = TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-7 * index),
                Cost = cost,
                Memory = _Memory,
                ShoppingListItemID = shoppingListItemID,
                ShoppingTripID = newestFirst.Length - index
            })
        ];

        return _Memory;
    }

    [Fact]
    public void Assess_ComparesPerUnitSoABiggerPackIsNotDearer()
    {
        var _Insight = ShoppingPriceLogic.Assess(Line(2, MeasurementUnitSE.Kilograms, 7.00m), Memory((4.00m, 1, MeasurementUnitSE.Kilograms)));

        _ = _Insight.IsDearerThanUsual.Should().BeFalse("$3.50 a kilo is cheaper than $4 a kilo, whatever the total");
        _ = _Insight.UsualCost.Should().Be(8.00m, "usual is said for this line's amount so it reads beside the line's price");
    }

    [Fact]
    public void Assess_ComparesGramsWithKilograms()
    {
        var _Insight = ShoppingPriceLogic.Assess(Line(1, MeasurementUnitSE.Kilograms, 5.00m), Memory((2.00m, 500, MeasurementUnitSE.Grams)));

        _ = _Insight.UsualCost.Should().Be(4.00m);
        _ = _Insight.IsDearerThanUsual.Should().BeTrue();
    }

    [Fact]
    public void Assess_ComparesMillilitresWithLitres()
    {
        var _Insight = ShoppingPriceLogic.Assess(Line(2, MeasurementUnitSE.Litres, 3.00m), Memory((0.75m, 500, MeasurementUnitSE.Millilitres)));

        _ = _Insight.UsualCost.Should().Be(3.00m);
    }

    [Fact]
    public void Assess_ComparesCountedUnitsOnlyWithThemselves()
    {
        var _Insight = ShoppingPriceLogic.Assess(Line(2, MeasurementUnitSE.Packets, 9.00m), Memory((1.00m, 1, MeasurementUnitSE.Tins)));

        _ = _Insight.UsualCost.Should().BeNull("a tin and a packet are not the same amount of anything");
        _ = _Insight.IsDearerThanUsual.Should().BeFalse();
    }

    [Fact]
    public void Assess_TreatsAnAmountWithNoUnitAsPieces()
    {
        var _Insight = ShoppingPriceLogic.Assess(Line(6, null, 3.00m), Memory((1.00m, 2, MeasurementUnitSE.Pieces)));

        _ = _Insight.UsualCost.Should().Be(3.00m);
    }

    [Fact]
    public void Assess_ComparesLinesWithNoAmountOnTheirWholePrice()
    {
        var _Insight = ShoppingPriceLogic.Assess(Line(null, null, 6.00m), Memory((5.00m, null, null)));

        _ = _Insight.UsualCost.Should().Be(5.00m);
        _ = _Insight.IsDearerThanUsual.Should().BeTrue();
    }

    [Fact]
    public void Assess_TakesTheMedianOfTheLastFivePurchasesSoOneSpecialDoesNotMoveIt()
    {
        var _Insight = ShoppingPriceLogic.Assess(
            Line(null, null, 4.00m),
            Memory((1.00m, null, null), (4.00m, null, null), (4.00m, null, null), (4.00m, null, null), (4.00m, null, null), (20.00m, null, null)));

        _ = _Insight.UsualCost.Should().Be(4.00m, "the $1 special is outvoted and the $20 purchase is too old to count");
    }

    [Fact]
    public void Assess_SplitsTheMiddleTwoWhenThereIsAnEvenNumberOfPurchases()
    {
        var _Insight = ShoppingPriceLogic.Assess(Line(null, null, null), Memory((4.00m, null, null), (5.00m, null, null)));

        _ = _Insight.UsualCost.Should().Be(4.50m);
    }

    [Fact]
    public void Assess_LeavesOutOnlyThePurchaseTheLinesOwnTickRecorded()
    {
        var _Insight = ShoppingPriceLogic.Assess(Line(null, null, 9.00m, shoppingListItemID: 100, tickedOnTripID: 2), ReusedLine(100, 9.00m, 4.00m));

        _ = _Insight.UsualCost.Should().Be(4.00m);
        _ = _Insight.IsDearerThanUsual.Should().BeTrue("a line in the trolley is judged against the shops before this one, not against itself");
    }

    [Fact]
    public void Assess_LeavesNothingOutForALineTickedOnAShopThatRecordedNothing()
    {
        var _Insight = ShoppingPriceLogic.Assess(Line(null, null, 5.00m, shoppingListItemID: 100, tickedOnTripID: 2), ReusedLine(100, 4.00m));

        _ = _Insight.UsualCost.Should().Be(4.00m, "the only purchase on this line was made on an earlier trip");
        _ = _Insight.IsDearerThanUsual.Should().BeTrue();
    }

    [Fact]
    public void Assess_JudgesALineKeptFromWeekToWeekAgainstItsEarlierShops()
    {
        var _Insight = ShoppingPriceLogic.Assess(Line(null, null, 5.00m, shoppingListItemID: 100), ReusedLine(100, 4.00m));

        _ = _Insight.UsualCost.Should().Be(4.00m, "last week's purchase on the same line is exactly what usual is for");
        _ = _Insight.IsDearerThanUsual.Should().BeTrue();
    }

    [Fact]
    public void Assess_GuessesALineKeptFromWeekToWeekFromItsLastShop()
        => _ = ShoppingPriceLogic.Assess(Line(null, null, null, shoppingListItemID: 100), ReusedLine(100, 4.00m))
            .EstimatedCost.Should().Be(4.00m);

    [Fact]
    public void Assess_FlagsOnlyWhatIsMoreThanTenPercentOverUsual()
    {
        _ = ShoppingPriceLogic.Assess(Line(null, null, 4.40m), Memory((4.00m, null, null))).IsDearerThanUsual.Should().BeFalse();
        _ = ShoppingPriceLogic.Assess(Line(null, null, 4.41m), Memory((4.00m, null, null))).IsDearerThanUsual.Should().BeTrue();
    }

    [Fact]
    public void Assess_GuessesAnUnpricedLineFromTheLastPurchaseScaledToItsAmount()
    {
        var _Insight = ShoppingPriceLogic.Assess(
            Line(3, MeasurementUnitSE.Litres, null),
            Memory((5.00m, 2, MeasurementUnitSE.Litres), (1.00m, 2, MeasurementUnitSE.Litres)));

        _ = _Insight.EstimatedCost.Should().Be(7.50m, "the last purchase was $2.50 a litre");
    }

    [Fact]
    public void Assess_TakesTheLastPriceAsItWasWhenTheUnitsCannotBeCompared()
    {
        var _Insight = ShoppingPriceLogic.Assess(Line(2, MeasurementUnitSE.Packets, null), Memory((3.20m, 1, MeasurementUnitSE.Tins)));

        _ = _Insight.EstimatedCost.Should().Be(3.20m);
    }

    [Fact]
    public void Assess_DoesNotGuessALineThatAlreadyHasAPrice()
        => _ = ShoppingPriceLogic.Assess(Line(null, null, 4.00m), Memory((5.00m, null, null))).EstimatedCost.Should().BeNull();

    [Fact]
    public void Assess_SaysNothingAboutAnItemNeverBought()
        => _ = ShoppingPriceLogic.Assess(Line(2, MeasurementUnitSE.Litres, 4.00m), null)
            .Should().Be(new ShoppingItemInsight(null, false, null, null));

    #endregion Methods

}
