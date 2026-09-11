using FluentAssertions;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingLists.GetShoppingList;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.ShoppingLists.GetShoppingList;
using Home.WebApi.UseCases.ShoppingLists.GetShoppingList;
using Home.WebApi.UseCases.ShoppingLists.Models;

namespace Home.Application.Tests.UseCases.ShoppingLists.GetShoppingList;

/// <summary>
/// What the household's memory adds to a list: what a line usually costs, whether this price is
/// dearer, a guess for a line with no price, and the aisle the item is filed under.
/// </summary>
public class PriceAndAisleTests : InteractorTest
{

    #region Fields

    private readonly GetShoppingListPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    /// <summary>
    /// A list holding a single line of milk, numbered ten past the list.
    /// </summary>
    private static ShoppingList BuildList(long shoppingListID, Household household, decimal? amount, MeasurementUnitSE? unit, decimal? cost)
    {
        var _List = new ShoppingList()
        {
            Household = household,
            Name = $"List {shoppingListID}",
            ShoppingListID = shoppingListID
        };

        _List.Items =
        [
            new ShoppingListItem()
            {
                Amount = amount,
                Cost = cost,
                Name = "Milk",
                Sequence = 1,
                ShoppingList = _List,
                ShoppingListItemID = shoppingListID + 10,
                Unit = unit?.Value
            }
        ];

        return _List;
    }

    private static ShoppingItemMemory BuildMemory(
        long shoppingItemMemoryID,
        Household household,
        ShoppingCategory? category,
        params (decimal Cost, decimal? Amount, MeasurementUnitSE? Unit, int DaysAgo)[] purchases)
    {
        var _Memory = new ShoppingItemMemory()
        {
            Household = household,
            Name = "Milk",
            NameKey = "milk",
            ShoppingCategory = category,
            ShoppingItemMemoryID = shoppingItemMemoryID
        };

        _Memory.Prices =
        [
            .. purchases.Select((p, index) => new ShoppingItemPrice()
            {
                Amount = p.Amount,
                BoughtOnUTC = TestServiceFactory.DefaultNow.UtcDateTime.AddDays(-p.DaysAgo),
                Cost = p.Cost,
                Memory = _Memory,
                ShoppingItemPriceID = shoppingItemMemoryID + 10 + index,
                ShoppingListItemID = shoppingItemMemoryID + 10 + index,
                Unit = p.Unit?.Value
            })
        ];

        return _Memory;
    }

    private Task HandleAsync(long shoppingListID)
    {
        var _Services = this.Services(out var _Context);

        return new GetShoppingListInteractor().HandleAsync(
            new GetShoppingListInputPort(shoppingListID),
            this.m_Presenter,
            _Services.With<IShoppingItemMemoryLogic>(new ShoppingItemMemoryLogic(_Context, _Services.Time)).Build(),
            CancellationToken.None);
    }

    private ShoppingListItemDto Line()
        => Ok<GetShoppingListApiResponse>(this.m_Presenter).Items.Single();

    [Fact]
    public async Task HandleAsync_SaysWhatTheLineUsuallyCostsAndFlagsAPriceWellOverIt()
    {
        _ = this.Database.Seed(
            BuildMemory(140, this.Ours, null, (4.00m, 2, MeasurementUnitSE.Litres, 7), (4.00m, 2, MeasurementUnitSE.Litres, 14)),
            BuildList(120, this.Ours, 2, MeasurementUnitSE.Litres, 5.00m));

        await this.HandleAsync(120);

        var _Line = this.Line();

        _ = _Line.UsualCost.Should().Be(4.00m);
        _ = _Line.IsDearerThanUsual.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_LeavesAPriceWithinTenPercentOfUsualUnflagged()
    {
        _ = this.Database.Seed(
            BuildMemory(140, this.Ours, null, (4.00m, 2, MeasurementUnitSE.Litres, 7)),
            BuildList(120, this.Ours, 2, MeasurementUnitSE.Litres, 4.30m));

        await this.HandleAsync(120);

        _ = this.Line().IsDearerThanUsual.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_GuessesALineWithNoPriceFromTheLastPurchaseWorkedOutPerKilo()
    {
        _ = this.Database.Seed(
            BuildMemory(140, this.Ours, null, (10.00m, 1, MeasurementUnitSE.Kilograms, 14), (16.00m, 2, MeasurementUnitSE.Kilograms, 7)),
            BuildList(120, this.Ours, 500, MeasurementUnitSE.Grams, null));

        await this.HandleAsync(120);

        _ = this.Line().EstimatedCost.Should().Be(4.00m, "the last purchase was $8 a kilo and this line is half a kilo");
    }

    [Fact]
    public async Task HandleAsync_CarriesTheAisleTheItemIsFiledUnder()
    {
        var _Dairy = new ShoppingCategory() { Household = this.Ours, Name = "Dairy", ShoppingCategoryID = 110 };

        _ = this.Database.Seed(BuildMemory(140, this.Ours, _Dairy), BuildList(120, this.Ours, 2, MeasurementUnitSE.Litres, null));

        await this.HandleAsync(120);

        _ = this.Line().ShoppingCategoryID.Should().Be(110);
    }

    [Fact]
    public async Task HandleAsync_NeverReadsAnotherHouseholdsMemoryOfTheSameItem()
    {
        var _TheirDairy = new ShoppingCategory() { Household = this.Theirs, Name = "Dairy", ShoppingCategoryID = 910 };

        _ = this.Database.Seed(
            BuildMemory(940, this.Theirs, _TheirDairy, (1.00m, 2, MeasurementUnitSE.Litres, 7)),
            BuildList(120, this.Ours, 2, MeasurementUnitSE.Litres, 5.00m));

        await this.HandleAsync(120);

        var _Line = this.Line();

        _ = _Line.ShoppingCategoryID.Should().BeNull();
        _ = _Line.UsualCost.Should().BeNull();
        _ = _Line.IsDearerThanUsual.Should().BeFalse("their cheap milk says nothing about ours");
    }

    [Fact]
    public async Task HandleAsync_SaysWhetherTheListIsGroupedByAisle()
    {
        var _List = BuildList(120, this.Ours, 2, MeasurementUnitSE.Litres, null);

        _List.GroupByAisle = true;

        _ = this.Database.Seed(_List);

        await this.HandleAsync(120);

        _ = Ok<GetShoppingListApiResponse>(this.m_Presenter).GroupByAisle.Should().BeTrue();
    }

    #endregion Methods

}
