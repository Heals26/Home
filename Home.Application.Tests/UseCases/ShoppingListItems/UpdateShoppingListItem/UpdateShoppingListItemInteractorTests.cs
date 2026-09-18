using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.ShoppingListItems.UpdateShoppingListItem;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ShoppingListItems.UpdateShoppingListItem;

/// <summary>
/// The sheet behind a line on a shopping list. Ticking it and moving it are their own use cases, so
/// what is left here is a form someone filled in, and every field left out of it stays as it was.
/// </summary>
public class UpdateShoppingListItemInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateShoppingListItemPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ShoppingList BuildList(long shoppingListID, Household household, params (long ItemID, string Name, long Sequence)[] items)
    {
        var _List = new ShoppingList()
        {
            Household = household,
            Name = $"List {shoppingListID}",
            ShoppingListID = shoppingListID
        };

        _List.Items =
        [
            .. items.Select(i => new ShoppingListItem()
            {
                Amount = 1,
                Name = i.Name,
                Sequence = i.Sequence,
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
        PropertyChangeTracker<string> name = default,
        PropertyChangeTracker<string?> note = default,
        PropertyChangeTracker<long?> unit = default)
    {
        var _Services = this.Services(out var _Context);

        return new UpdateShoppingListItemInteractor().HandleAsync(
            new UpdateShoppingListItemInputPort(amount, cost, name, note, shoppingListItemID, unit),
            this.m_Presenter,
            _Services
                .With<IShoppingListLogic>(new ShoppingListLogic(_Context))
                .With<IShoppingItemMemoryLogic>(new ShoppingItemMemoryLogic(_Context, _Services.Time))
                .With<IShoppingTripLogic>(new ShoppingTripLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_RewritesTheAmountAndPrice()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 1)));

        await this.HandleAsync(130, amount: new(2), cost: new(4.80m), name: new("Full cream milk"));

        var _Stored = this.Stored<ShoppingListItem>().Single();

        _ = _Stored.Amount.Should().Be(2);
        _ = _Stored.Cost.Should().Be(4.80m);
        _ = _Stored.Name.Should().Be("Full cream milk");
    }

    [Fact]
    public async Task HandleAsync_WhenOnlyOneFieldIsSent_LeavesEverythingElseAlone()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 3)));

        await this.HandleAsync(130, cost: new(4.80m));

        var _Stored = this.Stored<ShoppingListItem>().Single();

        _ = _Stored.Name.Should().Be("Milk");
        _ = _Stored.Sequence.Should().Be(3);
        _ = _Stored.Amount.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_KeepsWhatTheShopperNeedsToKnowAgainstTheLine()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Olive oil", 1)));

        await this.HandleAsync(130, note: new("  The tall green bottle  "));

        _ = this.Stored<ShoppingListItem>().Single().Note.Should().Be(
            "The tall green bottle",
            "the note is trimmed, because it is read at a shelf and not stored as typed");
    }

    [Fact]
    public async Task HandleAsync_AnEmptiedNoteLeavesNothingBehindRatherThanAnEmptyOne()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Olive oil", 1)));

        await this.HandleAsync(130, note: new("Woolies brand"));
        await this.HandleAsync(130, note: new("   "));

        _ = this.Stored<ShoppingListItem>().Single().Note.Should().BeNull(
            "a line either carries something worth reading or carries nothing");
    }

    [Fact]
    public async Task HandleAsync_LeavesTheNoteAloneWhenTheUpdateIsAboutSomethingElse()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Olive oil", 1)));

        await this.HandleAsync(130, note: new("Woolies brand"));
        await this.HandleAsync(130, cost: new(9.50m));

        _ = this.Stored<ShoppingListItem>().Single().Note.Should().Be("Woolies brand");
    }

    [Fact]
    public async Task HandleAsync_CanClearAnAmountAndUnitForSomethingBoughtByEye()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 1)));

        await this.HandleAsync(130, amount: new(null), unit: new(null));

        var _Stored = this.Stored<ShoppingListItem>().Single();

        _ = _Stored.Amount.Should().BeNull();
        _ = _Stored.Unit.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenTheItemBelongsToAnotherHousehold_RefusesBeforeReachingTheWrite()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, (130, "Milk", 1)),
            BuildList(920, this.Theirs, (930, "Caviar", 1)));

        await this.HandleAsync(930, name: new("Renamed by us"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ShoppingListItem>().Single(i => i.ShoppingListItemID == 930).Name.Should().Be("Caviar");
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchItemExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 1)));

        await this.HandleAsync(404, name: new("Anything"));

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
