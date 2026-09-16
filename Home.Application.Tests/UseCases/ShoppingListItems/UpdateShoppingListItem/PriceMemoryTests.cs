using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;
using Home.Application.UseCases.ShoppingLists.EndShoppingTrip;
using Home.Application.UseCases.ShoppingLists.StartShoppingTrip;
using Home.Application.UseCases.ShoppingLists.UntickShoppingListItems;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.ShoppingListItems.UpdateShoppingListItem;
using Home.WebApi.Presenters.ShoppingLists.EndShoppingTrip;
using Home.WebApi.Presenters.ShoppingLists.StartShoppingTrip;
using Home.WebApi.Presenters.ShoppingLists.UntickShoppingListItems;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ShoppingListItems.UpdateShoppingListItem;

/// <summary>
/// What a tick teaches the household's memory (11 Sep, tied to shops 16 Sep). A priced line ticked
/// during a shop is a purchase. While the shop goes on an untick takes it back, because that was a
/// mis-tap; once the shop is over the purchase stands, and a price typed in for a line still ticked
/// corrects it rather than adding another.
/// </summary>
public class PriceMemoryTests : InteractorTest
{

    #region Fields

    private readonly UpdateShoppingListItemPresenter m_Presenter = new(Mapper);

    /// <summary>
    /// How long after <see cref="TestServiceFactory.DefaultNow"/> the next request happens.
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

    private Task FinishShoppingAsync(long shoppingListID)
    {
        var _Services = this.Services(out var _Context);

        _Services.Time.Advance(this.m_Elapsed);

        return new EndShoppingTripInteractor().HandleAsync(
            new EndShoppingTripInputPort(shoppingListID),
            new EndShoppingTripPresenter(Mapper),
            _Services.With<IShoppingTripLogic>(new ShoppingTripLogic(_Context, _Services.Time)).Build(),
            CancellationToken.None);
    }

    private Task HandleAsync(
        long shoppingListItemID,
        PropertyChangeTracker<decimal?> amount = default,
        PropertyChangeTracker<decimal?> cost = default,
        PropertyChangeTracker<bool> inBasket = default,
        PropertyChangeTracker<string> name = default)
    {
        var _Services = this.Services(out var _Context);

        _Services.Time.Advance(this.m_Elapsed);

        return new UpdateShoppingListItemInteractor().HandleAsync(
            new UpdateShoppingListItemInputPort(amount, cost, inBasket, name, default, default, shoppingListItemID, default),
            this.m_Presenter,
            _Services
                .With<IShoppingListLogic>(new ShoppingListLogic(_Context))
                .With<IShoppingItemMemoryLogic>(new ShoppingItemMemoryLogic(_Context, _Services.Time))
                .With<IShoppingTripLogic>(new ShoppingTripLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    private Task StartShoppingAsync(long shoppingListID)
    {
        var _Services = this.Services(out var _Context);

        _Services.Time.Advance(this.m_Elapsed);

        return new StartShoppingTripInteractor().HandleAsync(
            new StartShoppingTripInputPort(shoppingListID),
            new StartShoppingTripPresenter(Mapper),
            _Services.With<IShoppingTripLogic>(new ShoppingTripLogic(_Context, _Services.Time)).Build(),
            CancellationToken.None);
    }

    private Task UntickAllAsync(long shoppingListID)
    {
        var _Services = this.Services(out var _Context);

        _Services.Time.Advance(this.m_Elapsed);

        return new UntickShoppingListItemsInteractor().HandleAsync(
            new UntickShoppingListItemsInputPort(shoppingListID),
            new UntickShoppingListItemsPresenter(Mapper),
            _Services.With<IShoppingTripLogic>(new ShoppingTripLogic(_Context, _Services.Time)).Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_TickingAPricedLineDuringAShopRemembersWhatItCost()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));

        var _Memory = this.Stored<ShoppingItemMemory>().Single(m => m.Household.HouseholdID == OurHouseholdID);
        var _Price = this.Stored<ShoppingItemPrice>().Single();
        var _Trip = this.Stored<ShoppingTrip>().Single();

        _ = _Memory.Name.Should().Be("Milk");
        _ = _Memory.NameKey.Should().Be("milk");
        _ = _Price.Amount.Should().Be(2);
        _ = _Price.BoughtOnUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
        _ = _Price.Cost.Should().Be(4.80m);
        _ = _Price.ShoppingListItemID.Should().Be(130);
        _ = _Price.ShoppingTripID.Should().Be(_Trip.ShoppingTripID);
        _ = _Price.Unit.Should().Be(MeasurementUnitSE.Litres.Value);
        _ = this.Stored<ShoppingListItem>().Single().ShoppingTripID.Should().Be(_Trip.ShoppingTripID);
    }

    [Fact]
    public async Task HandleAsync_TickingOutsideAShopRemembersNothing()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.HandleAsync(130, inBasket: new(true));

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingItemPrice>().Should().BeEmpty("only a shop remembers prices");
        _ = this.Stored<ShoppingListItem>().Single().InBasket.Should().BeTrue("the tick itself still counts");
    }

    [Fact]
    public async Task HandleAsync_TickingOnceTheShopHasGoneQuietRemembersNothing()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.StartShoppingAsync(120);
        this.m_Elapsed = ShoppingTripLogic.QuietSpell + TimeSpan.FromMinutes(1);
        await this.HandleAsync(130, inBasket: new(true));

        _ = this.Stored<ShoppingItemPrice>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_EachChangeDuringTheShopKeepsItGoing()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m), (131, "Bread", 3.50m)));

        await this.StartShoppingAsync(120);
        this.m_Elapsed = TimeSpan.FromMinutes(90);
        await this.HandleAsync(131, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromMinutes(180);
        await this.HandleAsync(130, inBasket: new(true));

        _ = this.Stored<ShoppingItemPrice>().Should().HaveCount(2, "two quiet hours are counted from the last tick, not from the start");
    }

    [Fact]
    public async Task HandleAsync_TickingALineWithNoPriceRemembersNothing()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", null)));

        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));

        _ = this.Stored<ShoppingItemMemory>().Should().BeEmpty("there is nothing to learn from a line with no price");
    }

    [Fact]
    public async Task HandleAsync_APriceTypedInAfterTheTickIsStillRecorded()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", null)));

        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromMinutes(5);
        await this.HandleAsync(130, cost: new(4.80m));

        _ = this.Stored<ShoppingItemPrice>().Single().Cost.Should().Be(4.80m);
    }

    [Fact]
    public async Task HandleAsync_APriceTypedInFromTheReceiptAfterTheShopCountsForThatShop()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", null)));

        await this.StartShoppingAsync(120);
        this.m_Elapsed = TimeSpan.FromMinutes(20);
        await this.HandleAsync(130, inBasket: new(true));
        await this.FinishShoppingAsync(120);
        this.m_Elapsed = TimeSpan.FromDays(1);
        await this.HandleAsync(130, cost: new(4.80m));

        var _Price = this.Stored<ShoppingItemPrice>().Single();

        _ = _Price.ShoppingTripID.Should().Be(this.Stored<ShoppingTrip>().Single().ShoppingTripID);
        _ = _Price.BoughtOnUTC.Should().Be(
            TestServiceFactory.DefaultNow.UtcDateTime.AddMinutes(20),
            "it was bought when the shop last saw anything happen, not when the receipt was typed in");
    }

    [Fact]
    public async Task HandleAsync_ANewPriceCorrectsThePurchaseRatherThanAddingOne()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromMinutes(10);
        await this.HandleAsync(130, cost: new(5.20m));

        _ = this.Stored<ShoppingItemPrice>().Single().Cost.Should().Be(5.20m);
    }

    [Fact]
    public async Task HandleAsync_SavingATickedLineDaysLaterCorrectsItsPurchaseRatherThanAddingOne()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromDays(3);
        await this.HandleAsync(130, amount: new(2), cost: new(4.60m));

        _ = this.Stored<ShoppingItemPrice>().Should().ContainSingle("the line was bought once, however long after the shop its sheet is saved");
        _ = this.Stored<ShoppingItemPrice>().Single().Cost.Should().Be(4.60m);
    }

    [Fact]
    public async Task HandleAsync_UntickingDuringTheShopTakesThePurchaseBack()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromMinutes(10);
        await this.HandleAsync(130, inBasket: new(false));

        _ = this.Stored<ShoppingItemPrice>().Should().BeEmpty("a tick taken back during the shop was a mis-tap, not a purchase");
        _ = this.Stored<ShoppingListItem>().Single().ShoppingTripID.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_UntickingAfterDoneLeavesThePurchaseStanding()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromMinutes(40);
        await this.FinishShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(false));

        _ = this.Stored<ShoppingItemPrice>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_UntickingOnceTheShopHasGoneQuietLeavesThePurchaseStanding()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromDays(3);
        await this.HandleAsync(130, inBasket: new(false));

        _ = this.Stored<ShoppingItemPrice>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_AMisTapAfterUntickAllCannotTakeBackTheShopBefore()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromMinutes(30);
        await this.UntickAllAsync(120);
        await this.HandleAsync(130, inBasket: new(true));
        await this.HandleAsync(130, inBasket: new(false));

        _ = this.Stored<ShoppingItemPrice>().Should().ContainSingle("the 14 Sep dry run lost a shop's price exactly this way");
    }

    [Fact]
    public async Task HandleAsync_AMisTapOnNextWeeksShopTakesBackOnlyThatShop()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromDays(3);
        await this.HandleAsync(130, inBasket: new(false));
        this.m_Elapsed = TimeSpan.FromDays(7);
        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));
        await this.HandleAsync(130, inBasket: new(false));

        _ = this.Stored<ShoppingItemPrice>().Single().BoughtOnUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
    }

    [Fact]
    public async Task HandleAsync_TickingTheSameLineOnNextWeeksShopIsASecondPurchase()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromMinutes(30);
        await this.UntickAllAsync(120);
        this.m_Elapsed = TimeSpan.FromDays(7);
        await this.StartShoppingAsync(120);
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

        await this.StartShoppingAsync(120);
        await this.StartShoppingAsync(121);
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

        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));

        _ = this.Stored<ShoppingItemPrice>().Count(p => p.Memory.Household.HouseholdID == TheirHouseholdID).Should().Be(1);
        _ = this.Stored<ShoppingItemPrice>().Count(p => p.Memory.Household.HouseholdID == OurHouseholdID).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_RenamingATickedLineMovesItsPurchaseRatherThanRecordingASecond()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));
        this.m_Elapsed = TimeSpan.FromMinutes(10);
        await this.HandleAsync(130, name: new("Full cream milk"));

        _ = this.Stored<ShoppingItemPrice>().Should().ContainSingle("renaming what went into the trolley is not a second purchase");
        _ = this.Stored<ShoppingItemPrice>().Count(p => p.Memory.NameKey == "full cream milk").Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_UntickingARenamedLineStillTakesItsPurchaseBack()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.StartShoppingAsync(120);
        await this.HandleAsync(130, inBasket: new(true));
        await this.HandleAsync(130, name: new("Full cream milk"));
        await this.HandleAsync(130, inBasket: new(false));

        _ = this.Stored<ShoppingItemPrice>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenAnotherRequestRemembersTheItemFirst_CarriesOnWithTheirs()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 4.80m)));

        await this.StartShoppingAsync(120);

        var _Services = this.Services(out var _Context);
        var _Raced = new RacedPersistenceContext(_Context, () => this.Database.Seed(new ShoppingItemMemory()
        {
            Household = this.Ours,
            Name = "Milk",
            NameKey = "milk",
            ShoppingItemMemoryID = 140
        }));

        await new UpdateShoppingListItemInteractor().HandleAsync(
            new UpdateShoppingListItemInputPort(default, default, new(true), default, default, default, 130, default),
            this.m_Presenter,
            _Services
                .With<IPersistenceContext>(_Raced)
                .With<IShoppingListLogic>(new ShoppingListLogic(_Raced))
                .With<IShoppingItemMemoryLogic>(new ShoppingItemMemoryLogic(_Raced, _Services.Time))
                .With<IShoppingTripLogic>(new ShoppingTripLogic(_Raced, _Services.Time))
                .Build(),
            CancellationToken.None);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingItemMemory>().Should().ContainSingle("the memory the other request saved is the one kept");
        _ = this.Stored<ShoppingItemPrice>().Count(p => p.Memory.ShoppingItemMemoryID == 140).Should().Be(1);
        _ = this.Stored<ShoppingListItem>().Single().InBasket.Should().BeTrue("losing the race must not lose the tick");
    }

    #endregion Methods

}
