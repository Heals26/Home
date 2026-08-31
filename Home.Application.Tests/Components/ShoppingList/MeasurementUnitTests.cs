using FluentAssertions;
using Home.Application.Infrastructure.Recipes;
using Home.Domain.Enumerations;
using Home.WebUI.Components.Pages.ShoppingList;
using Home.WebUI.DataAccess.Recipes.Models;

namespace Home.Application.Tests.Components.ShoppingList;

/// <summary>
/// A measurement is written down in three places that cannot see each other: the domain
/// enumeration, the web app's mirror of it, and the synonyms the shopping list parser accepts.
/// Until these tests existed a comment was the only thing holding them together, so a unit added
/// to one and forgotten in the others failed silently — an empty dropdown entry, or a line that
/// parsed its amount and dropped its unit. These pin all three to the enumeration.
/// </summary>
public class MeasurementUnitTests
{

    #region Methods

    [Fact]
    public void EveryUnitCanBeTypedOntoAShoppingList()
    {
        var _Unreachable = BaseEnumeration.GetAll<MeasurementUnitSE>()
            .Where(u => u.Abbreviation.Length > 0)
            .Where(u => ShoppingListItemLogic.Parse($"2 {u.Abbreviation} thing").Unit != u.Value)
            .Select(u => u.Name);

        _Unreachable.Should().BeEmpty("the parser's synonyms must cover every unit's own wording");
    }

    [Fact]
    public void EveryUnitReadsSingularBesideExactlyOne()
    {
        foreach (var _Unit in BaseEnumeration.GetAll<MeasurementUnitSE>())
        {
            MeasurementUnitLogic.GetAbbreviation(_Unit.Value, 1)
                .Should().Be(_Unit.SingularAbbreviation, $"one {_Unit.Name} is singular");

            MeasurementUnitLogic.GetAbbreviation(_Unit.Value, 2)
                .Should().Be(_Unit.Abbreviation, $"two {_Unit.Name} is plural");

            MeasurementUnitLogic.GetAbbreviation(_Unit.Value, 0.5m)
                .Should().Be(_Unit.Abbreviation, $"half a {_Unit.Name} is plural");

            MeasurementUnitLogic.GetAbbreviation(_Unit.Value, null)
                .Should().Be(_Unit.Abbreviation, $"an unstated number of {_Unit.Name} is plural");
        }
    }

    [Fact]
    public void TheWebAppMirrorsTheDomainExactly()
    {
        var _Domain = BaseEnumeration.GetAll<MeasurementUnitSE>()
            .OrderBy(u => u.Value)
            .Select(u => new { u.Abbreviation, u.Name, u.SingularAbbreviation, u.Value });

        var _WebApp = MeasurementUnits.All
            .OrderBy(u => u.Value)
            .Select(u => new { u.Abbreviation, u.Name, u.SingularAbbreviation, u.Value });

        _WebApp.Should().BeEquivalentTo(_Domain, o => o.WithStrictOrdering());
    }

    #endregion Methods

}
