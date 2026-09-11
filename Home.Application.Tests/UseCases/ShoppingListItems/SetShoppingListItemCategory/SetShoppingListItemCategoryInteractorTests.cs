using FluentAssertions;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingListItems.SetShoppingListItemCategory;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ShoppingListItems.SetShoppingListItemCategory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Tests.UseCases.ShoppingListItems.SetShoppingListItemCategory;

/// <summary>
/// Filing an item under an aisle. The aisle is remembered against the item's name rather than the
/// line, so once "Milk" is in Dairy it lands there on every list from then on.
/// </summary>
public class SetShoppingListItemCategoryInteractorTests : InteractorTest
{

    #region Fields

    private readonly SetShoppingListItemCategoryPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ShoppingCategory BuildCategory(long shoppingCategoryID, Household household, string name)
        => new()
        {
            Household = household,
            Name = name,
            ShoppingCategoryID = shoppingCategoryID
        };

    /// <summary>
    /// A list holding the one item, numbered ten past the list.
    /// </summary>
    private static ShoppingList BuildList(long shoppingListID, Household household, string itemName)
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
                Name = itemName,
                Sequence = 1,
                ShoppingList = _List,
                ShoppingListItemID = shoppingListID + 10
            }
        ];

        return _List;
    }

    private static ShoppingItemMemory BuildMemory(long shoppingItemMemoryID, Household household, string name, ShoppingCategory? category)
        => new()
        {
            Household = household,
            Name = name,
            NameKey = name.ToLowerInvariant(),
            ShoppingCategory = category,
            ShoppingItemMemoryID = shoppingItemMemoryID
        };

    /// <summary>
    /// Read off the key rather than the navigation, because a join onto a missing aisle comes back
    /// null whether or not anything was written.
    /// </summary>
    private IQueryable<ShoppingItemMemory> FiledUnder(long? shoppingCategoryID)
        => this.Stored<ShoppingItemMemory>().Where(m => EF.Property<long?>(m, "ShoppingCategoryID") == shoppingCategoryID);

    private Task HandleAsync(long shoppingListItemID, long? shoppingCategoryID)
    {
        var _Services = this.Services(out var _Context);

        return new SetShoppingListItemCategoryInteractor().HandleAsync(
            new SetShoppingListItemCategoryInputPort(shoppingCategoryID, shoppingListItemID),
            this.m_Presenter,
            _Services.With<IShoppingItemMemoryLogic>(new ShoppingItemMemoryLogic(_Context, _Services.Time)).Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_FilesTheItemUnderTheAisleByItsName()
    {
        _ = this.Database.Seed(BuildCategory(110, this.Ours, "Dairy"), BuildList(120, this.Ours, "Milk"));

        await this.HandleAsync(130, 110);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.FiledUnder(110)
            .Count(m => m.Household.HouseholdID == OurHouseholdID && m.NameKey == "milk")
            .Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_RefilesAnItemAlreadyRememberedWhateverTheCaseOfTheLine()
    {
        _ = this.Database.Seed(
            BuildCategory(110, this.Ours, "Dairy"),
            BuildMemory(140, this.Ours, "Milk", BuildCategory(111, this.Ours, "Frozen")),
            BuildList(120, this.Ours, "MILK"));

        await this.HandleAsync(130, 110);

        _ = this.Stored<ShoppingItemMemory>().Should().ContainSingle("MILK and Milk are the same thing to buy");
        _ = this.FiledUnder(110).Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WithNoAisleTakesTheItemOutOfTheOneItWasIn()
    {
        _ = this.Database.Seed(
            BuildMemory(140, this.Ours, "Milk", BuildCategory(110, this.Ours, "Dairy")),
            BuildList(120, this.Ours, "Milk"));

        await this.HandleAsync(130, null);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.FiledUnder(null).Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_LeavesAnotherHouseholdsMemoryOfTheSameItemAlone()
    {
        _ = this.Database.Seed(
            BuildCategory(110, this.Ours, "Chilled"),
            BuildMemory(940, this.Theirs, "Milk", BuildCategory(910, this.Theirs, "Dairy")),
            BuildList(120, this.Ours, "Milk"));

        await this.HandleAsync(130, 110);

        _ = this.Stored<ShoppingItemMemory>().Should().HaveCount(2, "each household remembers its own shop");
        _ = this.FiledUnder(910).Should().ContainSingle();
        _ = this.FiledUnder(110).Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenTheAisleBelongsToAnotherHousehold_PresentsNotFoundAndFilesNothing()
    {
        _ = this.Database.Seed(BuildCategory(910, this.Theirs, "Dairy"), BuildList(120, this.Ours, "Milk"));

        await this.HandleAsync(130, 910);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ShoppingItemMemory>().Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenTheItemBelongsToAnotherHousehold_PresentsNotFoundAndFilesNothing()
    {
        _ = this.Database.Seed(BuildCategory(110, this.Ours, "Dairy"), BuildList(920, this.Theirs, "Caviar"));

        await this.HandleAsync(930, 110);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ShoppingItemMemory>().Should().BeEmpty();
    }

    #endregion Methods

}
