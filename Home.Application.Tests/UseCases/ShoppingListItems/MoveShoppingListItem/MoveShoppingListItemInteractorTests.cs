using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingListItems.MoveShoppingListItem;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ShoppingListItems.MoveShoppingListItem;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ShoppingListItems.MoveShoppingListItem;

/// <summary>
/// Dragging an item onto another list. A move touches two lists, so both of them are checked
/// against the household rather than only the one being written to.
/// </summary>
public class MoveShoppingListItemInteractorTests : InteractorTest
{

    #region Fields

    private readonly MoveShoppingListItemPresenter m_Presenter = new(Mapper);

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
                Name = i.Name,
                Sequence = i.Sequence,
                ShoppingList = _List,
                ShoppingListItemID = i.ItemID
            })
        ];

        return _List;
    }

    /// <summary>
    /// How many items a list holds, counted in the query rather than off a loaded navigation.
    /// Reading <c>Stored&lt;ShoppingList&gt;().Single().Items</c> is always empty, so it would agree
    /// with whatever the interactor did.
    /// </summary>
    private int ItemsOn(long shoppingListID)
        => this.Stored<ShoppingListItem>().Count(i => i.ShoppingList.ShoppingListID == shoppingListID);

    private Task HandleAsync(long shoppingListItemID, long shoppingListID)
        => new MoveShoppingListItemInteractor().HandleAsync(
            new MoveShoppingListItemInputPort(shoppingListID, shoppingListItemID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_PutsTheItemOnTheOtherListAtTheEndOfIt()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, (130, "Milk", 1), (131, "Bread", 2)),
            BuildList(121, this.Ours, (140, "Screws", 1), (141, "Paint", 2)));

        await this.HandleAsync(130, 121);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();

        var _Moved = this.Stored<ShoppingListItem>().Single(i => i.ShoppingListItemID == 130);

        _ = _Moved.Sequence.Should().Be(
            3,
            "dropping onto a list says which list, not where in it, so it lands where a new item would");
    }

    [Fact]
    public async Task HandleAsync_TakesTheItemOffTheListItCameFrom()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, (130, "Milk", 1), (131, "Bread", 2)),
            BuildList(121, this.Ours, (140, "Screws", 1)));

        await this.HandleAsync(130, 121);

        _ = this.ItemsOn(120).Should().Be(
            1,
            "the item left this list rather than being copied off it");
        _ = this.ItemsOn(121).Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_DoesNothingWhenItIsDroppedOnTheListItIsAlreadyOn()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 1), (131, "Bread", 2)));

        await this.HandleAsync(130, 120);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingListItem>().Single(i => i.ShoppingListItemID == 130).Sequence.Should().Be(
            1,
            "a move onto its own list must not renumber it to the end");
    }

    [Fact]
    public async Task HandleAsync_RefusesToMoveOntoAnotherHouseholdsList()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, (130, "Milk", 1)),
            BuildList(920, this.Theirs, (940, "Their milk", 1)));

        await this.HandleAsync(130, 920);

        _ = this.m_Presenter.Result.Should().BeOfType<NotFoundObjectResult>();
        _ = this.ItemsOn(920).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_RefusesToMoveAnotherHouseholdsItem()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, (130, "Milk", 1)),
            BuildList(920, this.Theirs, (940, "Their milk", 1)));

        await this.HandleAsync(940, 120);

        _ = this.m_Presenter.Result.Should().BeOfType<NotFoundObjectResult>();
        _ = this.ItemsOn(120).Should().Be(1);
    }

    #endregion Methods

}
