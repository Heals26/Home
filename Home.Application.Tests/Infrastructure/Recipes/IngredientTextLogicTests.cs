using FluentAssertions;
using Home.Application.Infrastructure.Recipes;
using Home.Domain.Enumerations;

namespace Home.Application.Tests.Infrastructure.Recipes;

/// <summary>
/// Reading an imported recipe's ingredient line. A site writes one line per ingredient and the
/// amount is inside it, so this is what decides whether a recipe arrives able to be scaled, priced
/// and shopped for, or as a list of sentences.
/// </summary>
public class IngredientTextLogicTests
{

    #region Methods

    [Theory]
    [InlineData("2 cups plain flour", 2, "Plain flour", 8)]
    [InlineData("500g beef mince", 500, "Beef mince", 2)]
    [InlineData("1 kg potatoes", 1, "Potatoes", 3)]
    [InlineData("1/2 cup rice", 0.5, "Rice", 8)]
    [InlineData("2 tbsp olive oil", 2, "Olive oil", 7)]
    [InlineData("3 cloves garlic, crushed", 3, "Garlic, crushed", 12)]
    [InlineData("1 tin diced tomatoes", 1, "Diced tomatoes", 13)]
    public void Parse_TakesTheAmountAndTheUnitOffTheFront(string line, double amount, string name, long unit)
    {
        var _Parsed = IngredientTextLogic.Parse(line);

        _ = _Parsed.Amount.Should().Be((decimal)amount);
        _ = _Parsed.Name.Should().Be(name);
        _ = _Parsed.Unit.Should().Be(unit);
    }

    [Theory]
    [InlineData("2 eggs", 2, "Eggs")]
    [InlineData("2 x eggs", 2, "Eggs")]
    [InlineData("6 bread rolls", 6, "Bread rolls")]
    public void Parse_ReadsACountWithNoMeasurement(string line, double amount, string name)
    {
        var _Parsed = IngredientTextLogic.Parse(line);

        _ = _Parsed.Amount.Should().Be((decimal)amount);
        _ = _Parsed.Name.Should().Be(name);
        _ = _Parsed.Unit.Should().BeNull("a count of somethings is an amount with no measurement");
    }

    [Theory]
    [InlineData("Salt and pepper to taste")]
    [InlineData("Olive oil")]
    [InlineData("A handful of parsley")]
    public void Parse_LeavesALineThatNamesNoAmountAlone(string line)
    {
        var _Parsed = IngredientTextLogic.Parse(line);

        _ = _Parsed.Amount.Should().BeNull();
        _ = _Parsed.Name.Should().Be(line);
        _ = _Parsed.Unit.Should().BeNull();
    }

    [Fact]
    public void Parse_KeepsALineThatIsOnlyAnAmountWhole()
    {
        var _Parsed = IngredientTextLogic.Parse("500g");

        _ = _Parsed.Amount.Should().BeNull("an amount with nothing left to buy is the name of the thing");
        _ = _Parsed.Name.Should().Be("500g");
    }

    [Fact]
    public void Parse_KeepsALineWhoseWordIsNotAMeasurementWhole()
    {
        var _Parsed = IngredientTextLogic.Parse("2kfc buckets");

        _ = _Parsed.Amount.Should().BeNull();
        _ = _Parsed.Name.Should().Be("2kfc buckets");
    }

    [Fact]
    public void Parse_TidiesTheSpacingAndTheFirstLetter()
    {
        var _Parsed = IngredientTextLogic.Parse("  2   cups    plain   flour  ");

        _ = _Parsed.Name.Should().Be("Plain flour");
    }

    [Fact]
    public void Parse_ReadsEveryMeasurementTheAppStores()
    {
        foreach (var _Unit in BaseEnumeration.GetAll<MeasurementUnitSE>().Where(u => u.Abbreviation.Length > 0))
        {
            var _Parsed = IngredientTextLogic.Parse($"2 {_Unit.Abbreviation} thing");

            _ = _Parsed.Unit.Should().Be(_Unit.Value, $"'{_Unit.Abbreviation}' is how the app itself writes {_Unit.Name}");
        }
    }

    [Fact]
    public void Parse_ReadsNothingFromNothing()
    {
        var _Parsed = IngredientTextLogic.Parse("   ");

        _ = _Parsed.Amount.Should().BeNull();
        _ = _Parsed.Name.Should().BeEmpty();
        _ = _Parsed.Unit.Should().BeNull();
    }

    #endregion Methods

}
