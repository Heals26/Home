using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingLists.DuplicateShoppingList;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.ShoppingLists.DuplicateShoppingList;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ShoppingLists.DuplicateShoppingList;

/// <summary>
/// "This week's like last week's". The copy carries the same things to buy, but nothing arrives
/// ticked and nothing arrives priced: that would be last week's trolley and last week's receipt.
/// </summary>
public class DuplicateShoppingListInteractorTests : InteractorTest
{

    #region Fields

    private readonly DuplicateShoppingListPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ShoppingList BuildList(long shoppingListID, Household household, string name, bool isArchived = false)
    {
        var _List = new ShoppingList()
        {
            Household = household,
            IsArchived = isArchived,
            Name = name,
            ShoppingListID = shoppingListID
        };

        _List.Items =
        [
            new ShoppingListItem()
            {
                Amount = 2,
                Cost = 4.80m,
                InBasket = true,
                Name = "Milk",
                Sequence = 2,
                ShoppingList = _List,
                ShoppingListItemID = shoppingListID + 1,
                Unit = MeasurementUnitSE.Litres.Value
            },
            new ShoppingListItem()
            {
                Amount = 1,
                Cost = 3.20m,
                InBasket = false,
                Name = "Bread",
                Sequence = 1,
                ShoppingList = _List,
                ShoppingListItemID = shoppingListID + 2
            }
        ];

        return _List;
    }

    private Task HandleAsync(long shoppingListID, string name)
        => new DuplicateShoppingListInteractor().HandleAsync(
            new DuplicateShoppingListInputPort(name, shoppingListID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_CopiesTheThingsToBuyUnderTheNewName()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, "Last week"));

        await this.HandleAsync(120, "This week");

        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();

        var _Copy = this.Stored<ShoppingList>().Single(sl => sl.Name == "This week");

        _ = this.Stored<ShoppingListItem>().Count(i => i.ShoppingList.ShoppingListID == _Copy.ShoppingListID).Should().Be(2);
        _ = this.Stored<ShoppingListItem>()
            .Where(i => i.ShoppingList.ShoppingListID == _Copy.ShoppingListID)
            .Select(i => i.Name)
            .Should().BeEquivalentTo(["Milk", "Bread"]);
    }

    [Fact]
    public async Task HandleAsync_BringsNothingBackTicked()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, "Last week"));

        await this.HandleAsync(120, "This week");

        var _Copy = this.Stored<ShoppingList>().Single(sl => sl.Name == "This week");

        _ = this.Stored<ShoppingListItem>()
            .Where(i => i.ShoppingList.ShoppingListID == _Copy.ShoppingListID)
            .Should().OnlyContain(i => !i.InBasket, "the copy is a list to shop, not last week's trolley");
    }

    [Fact]
    public async Task HandleAsync_BringsNothingBackPriced()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, "Last week"));

        await this.HandleAsync(120, "This week");

        var _Copy = this.Stored<ShoppingList>().Single(sl => sl.Name == "This week");

        _ = this.Stored<ShoppingListItem>()
            .Where(i => i.ShoppingList.ShoppingListID == _Copy.ShoppingListID)
            .Should().OnlyContain(i => i.Cost == null, "prices belong to the shop that happened, not the next one");
    }

    [Fact]
    public async Task HandleAsync_KeepsTheAmountsAndTheOrderTheyWereWrittenIn()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, "Last week"));

        await this.HandleAsync(120, "This week");

        var _Copy = this.Stored<ShoppingList>().Single(sl => sl.Name == "This week");
        var _Milk = this.Stored<ShoppingListItem>()
            .Single(i => i.ShoppingList.ShoppingListID == _Copy.ShoppingListID && i.Name == "Milk");

        _ = _Milk.Amount.Should().Be(2);
        _ = _Milk.Unit.Should().Be(MeasurementUnitSE.Litres.Value);
        _ = _Milk.Sequence.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_LeavesTheOriginalUntouched()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, "Last week"));

        await this.HandleAsync(120, "This week");

        var _Original = this.Stored<ShoppingList>().Single(sl => sl.Name == "Last week");

        _ = this.Stored<ShoppingListItem>()
            .Count(i => i.ShoppingList.ShoppingListID == _Original.ShoppingListID && i.InBasket)
            .Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_CanCopyAnArchivedListWithoutBringingTheArchiveFlagAlong()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, "Christmas", isArchived: true));

        await this.HandleAsync(120, "Christmas again");

        _ = this.Stored<ShoppingList>().Single(sl => sl.Name == "Christmas again").IsArchived.Should().BeFalse(
            "last Christmas's shop can be duplicated a year later, and the copy is a live list");
    }

    [Fact]
    public async Task HandleAsync_WhenTheListBelongsToAnotherHousehold_PresentsNotFoundAndCopiesNothing()
    {
        _ = this.Database.Seed(BuildList(920, this.Theirs, "Theirs"));

        await this.HandleAsync(920, "Copied by us");

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ShoppingList>().Should().ContainSingle();
    }

    #endregion Methods

}
