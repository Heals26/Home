using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingListItems.GetShoppingListItem;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.ShoppingListItems.GetShoppingListItem;
using Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItem;

namespace Home.Application.Tests.UseCases.ShoppingListItems.GetShoppingListItem;

/// <summary>
/// One line on a shopping list, reached through its list to the household that owns it.
/// </summary>
public class GetShoppingListItemInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetShoppingListItemPresenter m_Presenter = new(Mapper);

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

    private Task HandleAsync(long shoppingListItemID)
        => new GetShoppingListItemInteractor().HandleAsync(
            new GetShoppingListItemInputPort(shoppingListItemID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_WhenTheLineIsOurs_PresentsItWithItsUnitSpelledForTheAmount()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, new ShoppingListItem()
        {
            Amount = 1,
            Cost = 4.5m,
            InBasket = true,
            Name = "Passata",
            Sequence = 2,
            ShoppingListItemID = 130,
            Unit = MeasurementUnitSE.Jars.Value
        }));

        await this.HandleAsync(130);

        var _Response = Ok<GetShoppingListItemApiResponse>(this.m_Presenter);

        _ = _Response.Name.Should().Be("Passata");
        _ = _Response.Amount.Should().Be(1);
        _ = _Response.Cost.Should().Be(4.5m);
        _ = _Response.InBasket.Should().BeTrue();
        _ = _Response.Sequence.Should().Be(2);
        _ = _Response.UnitAbbreviation.Should().Be("jar", "one of something never reads as a plural");
    }

    [Fact]
    public async Task HandleAsync_WhenTheLineBelongsToAnotherHousehold_PresentsNotFound()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, new ShoppingListItem() { Name = "Milk", Sequence = 1, ShoppingListItemID = 130 }),
            BuildList(920, this.Theirs, new ShoppingListItem() { Name = "Caviar", Sequence = 1, ShoppingListItemID = 930 }));

        await this.HandleAsync(930);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchLineExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, new ShoppingListItem() { Name = "Milk", Sequence = 1, ShoppingListItemID = 130 }));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
