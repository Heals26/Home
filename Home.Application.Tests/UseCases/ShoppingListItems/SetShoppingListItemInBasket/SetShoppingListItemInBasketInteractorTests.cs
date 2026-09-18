using FluentAssertions;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingListItems.SetShoppingListItemInBasket;
using Home.Application.UseCases.ShoppingListItems.UpdateShoppingListItem;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.ShoppingListItems.SetShoppingListItemInBasket;
using Home.WebApi.Presenters.ShoppingListItems.UpdateShoppingListItem;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ShoppingListItems.SetShoppingListItemInBasket;

/// <summary>
/// Ticking a line into the trolley and taking it back out, which is its own use case because during
/// a shop it is what records a purchase. What the tick teaches the household's memory is covered by
/// <see cref="UpdateShoppingListItem.PriceMemoryTests"/>.
/// </summary>
public class SetShoppingListItemInBasketInteractorTests : InteractorTest
{

    #region Fields

    private readonly SetShoppingListItemInBasketPresenter m_Presenter = new(Mapper);

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

    private Task HandleAsync(long shoppingListItemID, bool inBasket)
    {
        var _Services = this.Services(out var _Context);

        return new SetShoppingListItemInBasketInteractor().HandleAsync(
            new SetShoppingListItemInBasketInputPort(inBasket, shoppingListItemID),
            this.m_Presenter,
            _Services
                .With<IShoppingListLogic>(new ShoppingListLogic(_Context))
                .With<IShoppingItemMemoryLogic>(new ShoppingItemMemoryLogic(_Context, _Services.Time))
                .With<IShoppingTripLogic>(new ShoppingTripLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    /// <summary>
    /// The sheet behind the line, which is the other half of what used to be one endpoint.
    /// </summary>
    private Task SaveSheetAsync(long shoppingListItemID, PropertyChangeTracker<string?> note = default)
    {
        var _Services = this.Services(out var _Context);

        return new UpdateShoppingListItemInteractor().HandleAsync(
            new UpdateShoppingListItemInputPort(default, default, default, note, shoppingListItemID, default),
            new UpdateShoppingListItemPresenter(Mapper),
            _Services
                .With<IShoppingListLogic>(new ShoppingListLogic(_Context))
                .With<IShoppingItemMemoryLogic>(new ShoppingItemMemoryLogic(_Context, _Services.Time))
                .With<IShoppingTripLogic>(new ShoppingTripLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_TicksTheItemIntoTheBasket()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 1)));

        await this.HandleAsync(130, true);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingListItem>().Single().InBasket.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_TakesTheItemBackOutOfTheBasket()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 1)));

        await this.HandleAsync(130, true);
        await this.HandleAsync(130, false);

        _ = this.Stored<ShoppingListItem>().Single().InBasket.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_LeavesEverythingElseOnTheLineAlone()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 3)));

        await this.HandleAsync(130, true);

        var _Stored = this.Stored<ShoppingListItem>().Single();

        _ = _Stored.Name.Should().Be("Milk");
        _ = _Stored.Sequence.Should().Be(3);
        _ = _Stored.Amount.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_LeavesTheNoteAlone()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Olive oil", 1)));

        await this.SaveSheetAsync(130, note: new("Woolies brand"));
        await this.HandleAsync(130, true);

        _ = this.Stored<ShoppingListItem>().Single().Note.Should().Be(
            "Woolies brand",
            "ticking something off must not wipe what the line said");
    }

    [Fact]
    public async Task HandleAsync_WhenTheItemBelongsToAnotherHousehold_RefusesBeforeReachingTheWrite()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, (130, "Milk", 1)),
            BuildList(920, this.Theirs, (930, "Caviar", 1)));

        await this.HandleAsync(930, true);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ShoppingListItem>().Single(i => i.ShoppingListItemID == 930).InBasket.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchItemExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 1)));

        await this.HandleAsync(404, true);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
