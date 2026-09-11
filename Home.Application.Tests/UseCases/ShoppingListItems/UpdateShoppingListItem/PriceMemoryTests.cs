using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.ShoppingListItems.UpdateShoppingListItem;

namespace Home.Application.Tests.UseCases.ShoppingListItems.UpdateShoppingListItem;

/// <summary>
/// What a tick teaches the household's memory (11 Sep). A priced line going into the trolley is a
/// purchase. Within twelve hours a new price corrects it and an untick takes it back, because that
/// was a mis-tap; after that the purchase stands.
/// </summary>
public class PriceMemoryTests : InteractorTest
{

    #region Fields

    private readonly UpdateShoppingListItemPresenter m_Presenter = new(Mapper);

    /// <summary>
    /// How long after <see cref="TestServiceFactory.DefaultNow"/> the next update happens.
    /// </summary>
    private TimeSpan m_Elapsed;

    #endregion Fields

    #region Methods

    private static ShoppingList BuildList(long shoppingListID, Household household, params (long ItemID, string Name, decimal? Cost)[] items)
    {
        var _List = new ShoppingList()
        {
            Household = household,
            Name = $"List {shoppingListID}",
            ShoppingListID = shoppingListID
        };

        _List.Items =
        [
            .. items.Select((i, index) => new ShoppingListItem()
            {
                Amount = 2,
                Cost = i.Cost,
                Name = i.Name,
                Sequence = index + 1,
                ShoppingList = _List,
                ShoppingListItemID = i.ItemID,
                Unit = MeasurementUnitSE.Litres.Value
            })
        ];

        return _List;
    }

    private Task HandleAsync(
        long shoppingListItemID,
        PropertyChangeTracker<decimal?> amount = default,
        PropertyChangeTracker<decimal?> cost = default,
        PropertyChangeTracker<bool> inBasket = default)
    {
        var _Services = this.Services(out var _Context);

        _Services.Time.Advance(this.m_Elapsed);

        return new UpdateShoppingListItemInteractor().HandleAsync(
            new UpdateShoppingListItemInputPort(amount, cost, inBasket, default, default, default, shoppingListItemID, default),
            this.m_Presenter,
            _Services
                .With<IShoppingListLogic>(new ShoppingListLogic(_Context))
                .With<IShoppingItemMemoryLogic>(new ShoppingItemMemoryLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_TickingAPricedLineRemembersWhatItCost()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.HandleAsync(130, inBasket: new(true));

        var _Memory = this.Stored<ShoppingItemMemory>().Single(m => m.Household.HouseholdID == OurHouseholdID);
        var _Price = this.Stored<ShoppingItemPrice>().Single();

        _ = _Memory.Name.Should().Be("Milk");
        _ = _Memory.NameKey.Should().Be("milk");
        _ = _Price.Amount.Should().Be(2);
        _ = _Price.BoughtOnUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
        _ = _Price.Cost.Should().Be(4.80m);
        _ = _Price.ShoppingListItemID.Should().Be(130);
        _ = _Price.Unit.Should().Be(MeasurementUnitSE.Litres.Value);
    }

    [Fact]
    public async Task HandleAsync_TickingALineWithNoPriceRemembersNothing()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", null)));

        await this.HandleAsync(130, inBasket: new(true));

        _ = this.Stored<ShoppingItemMemory>().Should().BeEmpty("there is nothing to learn from a line with no price");
    }

    [Fact]
    public async Task HandleAsync_APriceTypedInAfterTheTickIsStillRecorded()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", null)));

        await this.HandleAsync(130, inBasket: new(true));
        await this.HandleAsync(130, cost: new(4.80m));

        _ = this.Stored<ShoppingItemPrice>().Single().Cost.Should().Be(4.80m);
    }

    [Fact]
    public async Task HandleAsync_ANewPriceWithinTwelveHoursCorrectsThePurchaseRatherThanAddingOne()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.HandleAsync(130, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromHours(1);
        await this.HandleAsync(130, cost: new(5.20m));

        _ = this.Stored<ShoppingItemPrice>().Single().Cost.Should().Be(5.20m);
    }

    [Fact]
    public async Task HandleAsync_UntickingWithinTwelveHoursTakesThePurchaseBack()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.HandleAsync(130, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromHours(1);
        await this.HandleAsync(130, inBasket: new(false));

        _ = this.Stored<ShoppingItemPrice>().Should().BeEmpty("a tick taken back straight away was a mis-tap, not a purchase");
    }

    [Fact]
    public async Task HandleAsync_UntickingDaysLaterLeavesThePurchaseStanding()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.HandleAsync(130, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromDays(3);
        await this.HandleAsync(130, inBasket: new(false));

        _ = this.Stored<ShoppingItemPrice>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_TickingTheSameLineNextWeekIsASecondPurchase()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.HandleAsync(130, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromDays(7);
        await this.HandleAsync(130, inBasket: new(false));
        await this.HandleAsync(130, cost: new(5.00m), inBasket: new(true));

        _ = this.Stored<ShoppingItemPrice>().OrderBy(p => p.BoughtOnUTC).Select(p => p.Cost).Should().Equal(
            [4.80m, 5.00m],
            "a list reused a week later is a new shop, not a correction of the last one");
    }

    [Fact]
    public async Task HandleAsync_TheSameItemOnAnyListFeedsOneMemoryWhateverItsCase()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, (130, "Milk", 4.80m)),
            BuildList(121, this.Ours, (131, "MILK", 5.00m)));

        await this.HandleAsync(130, inBasket: new(true));
        await this.HandleAsync(131, inBasket: new(true));

        _ = this.Stored<ShoppingItemMemory>().Should().ContainSingle();
        _ = this.Stored<ShoppingItemPrice>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_NeverWritesToAnotherHouseholdsMemoryOfTheSameItem()
    {
        var _Theirs = new ShoppingItemMemory()
        {
            Household = this.Theirs,
            Name = "Milk",
            NameKey = "milk",
            ShoppingItemMemoryID = 940
        };

        _Theirs.Prices =
        [
            new ShoppingItemPrice()
            {
                BoughtOnUTC = TestServiceFactory.DefaultNow.UtcDateTime,
                Cost = 9.99m,
                Memory = _Theirs,
                ShoppingItemPriceID = 950,
                ShoppingListItemID = 930
            }
        ];

        _ = this.Database.Seed(_Theirs, BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.HandleAsync(130, inBasket: new(true));

        _ = this.Stored<ShoppingItemPrice>().Count(p => p.Memory.Household.HouseholdID == TheirHouseholdID).Should().Be(1);
        _ = this.Stored<ShoppingItemPrice>().Count(p => p.Memory.Household.HouseholdID == OurHouseholdID).Should().Be(1);
    }

    #endregion Methods

}
