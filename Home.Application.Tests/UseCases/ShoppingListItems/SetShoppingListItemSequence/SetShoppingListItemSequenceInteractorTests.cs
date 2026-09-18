using FluentAssertions;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingListItems.SetShoppingListItemSequence;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.ShoppingListItems.SetShoppingListItemSequence;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ShoppingListItems.SetShoppingListItemSequence;

/// <summary>
/// Putting a line somewhere else in its list, which shoves the ones it passes and is the only write
/// in the application that changes rows the caller did not name.
/// </summary>
public class SetShoppingListItemSequenceInteractorTests : InteractorTest
{

    #region Fields

    private readonly SetShoppingListItemSequencePresenter m_Presenter = new(Mapper);

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

    private Task HandleAsync(long shoppingListItemID, long sequence)
    {
        var _Services = this.Services(out var _Context);

        return new SetShoppingListItemSequenceInteractor().HandleAsync(
            new SetShoppingListItemSequenceInputPort(sequence, shoppingListItemID),
            this.m_Presenter,
            _Services
                .With<IShoppingListLogic>(new ShoppingListLogic(_Context))
                .With<IShoppingTripLogic>(new ShoppingTripLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    /// <summary>
    /// The list in order, which is the only thing a reorder is judged by.
    /// </summary>
    private IEnumerable<string> StoredOrder()
        => this.Stored<ShoppingListItem>().OrderBy(i => i.Sequence).ThenBy(i => i.ShoppingListItemID).Select(i => i.Name);

    [Fact]
    public async Task HandleAsync_MovingAnItemUpTheListPutsItThereAndClosesTheGap()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 1), (131, "Bread", 2), (132, "Eggs", 3), (133, "Jam", 4)));

        await this.HandleAsync(133, 2);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.StoredOrder().Should().Equal(
            ["Milk", "Jam", "Bread", "Eggs"],
            "this used to move every other item and never the one being moved, so reordering did nothing");
    }

    [Fact]
    public async Task HandleAsync_MovingAnItemDownTheListWorksTheSameWayInReverse()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 1), (131, "Bread", 2), (132, "Eggs", 3), (133, "Jam", 4)));

        await this.HandleAsync(130, 3);

        _ = this.StoredOrder().Should().Equal(["Bread", "Eggs", "Milk", "Jam"]);
    }

    [Fact]
    public async Task HandleAsync_LeavesTheListAloneWhenSomethingIsDroppedWhereItAlreadyWas()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 1), (131, "Bread", 2), (132, "Eggs", 3)));

        await this.HandleAsync(131, 2);

        _ = this.Stored<ShoppingListItem>().Select(i => i.Sequence).Should().Equal(
            [1, 2, 3],
            "a move to where it already is must not renumber anything");
    }

    [Fact]
    public async Task HandleAsync_KeepsTheSequencesContiguousSoRepeatedMovesStayPredictable()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 1), (131, "Bread", 2), (132, "Eggs", 3), (133, "Jam", 4)));

        await this.HandleAsync(133, 1);
        await this.HandleAsync(130, 4);
        await this.HandleAsync(132, 2);

        _ = this.Stored<ShoppingListItem>().OrderBy(i => i.Sequence).Select(i => i.Sequence).Should().Equal(
            [1, 2, 3, 4],
            "nothing may drift apart, or a later move lands between two items instead of on one");
    }

    [Fact]
    public async Task HandleAsync_WhenTheItemBelongsToAnotherHousehold_RefusesBeforeReachingTheWrite()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, (130, "Milk", 1)),
            BuildList(920, this.Theirs, (930, "Caviar", 1), (931, "Truffles", 2)));

        await this.HandleAsync(931, 1);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ShoppingListItem>().Single(i => i.ShoppingListItemID == 931).Sequence.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchItemExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", 1)));

        await this.HandleAsync(404, 1);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
