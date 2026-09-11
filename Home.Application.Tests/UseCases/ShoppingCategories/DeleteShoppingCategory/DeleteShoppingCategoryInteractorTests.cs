using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingCategories.DeleteShoppingCategory;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ShoppingCategories.DeleteShoppingCategory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Tests.UseCases.ShoppingCategories.DeleteShoppingCategory;

/// <summary>
/// Removing an aisle. What was filed under it cannot cascade from here, so the items are let go of
/// by hand and wait to be filed again, keeping everything they cost.
/// </summary>
public class DeleteShoppingCategoryInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteShoppingCategoryPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ShoppingCategory BuildCategory(long shoppingCategoryID, Household household, string name)
        => new()
        {
            Household = household,
            Name = name,
            ShoppingCategoryID = shoppingCategoryID
        };

    private static ShoppingItemMemory BuildMemory(long shoppingItemMemoryID, Household household, string name, ShoppingCategory category)
    {
        var _Memory = new ShoppingItemMemory()
        {
            Household = household,
            Name = name,
            NameKey = name.ToLowerInvariant(),
            ShoppingCategory = category,
            ShoppingItemMemoryID = shoppingItemMemoryID
        };

        _Memory.Prices =
        [
            new ShoppingItemPrice()
            {
                BoughtOnUTC = TestServiceFactory.DefaultNow.UtcDateTime,
                Cost = 4.80m,
                Memory = _Memory,
                ShoppingItemPriceID = shoppingItemMemoryID + 10,
                ShoppingListItemID = shoppingItemMemoryID + 20
            }
        ];

        return _Memory;
    }

    /// <summary>
    /// Read off the key rather than the navigation, because a join onto a removed aisle comes back
    /// null whether or not the item was let go of.
    /// </summary>
    private int FiledUnder(long shoppingCategoryID)
        => this.Stored<ShoppingItemMemory>().Count(m => EF.Property<long?>(m, "ShoppingCategoryID") == shoppingCategoryID);

    private Task HandleAsync(long shoppingCategoryID)
        => new DeleteShoppingCategoryInteractor().HandleAsync(
            new DeleteShoppingCategoryInputPort(shoppingCategoryID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesTheAisleAndUnfilesWhatWasInIt()
    {
        _ = this.Database.Seed(BuildMemory(120, this.Ours, "Milk", BuildCategory(110, this.Ours, "Dairy")));

        await this.HandleAsync(110);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingCategory>().Should().BeEmpty();
        _ = this.FiledUnder(110).Should().Be(0, "the key does not cascade, so a filed item left pointing at the aisle fails the save");
        _ = this.Stored<ShoppingItemMemory>().Should().ContainSingle("the item is still remembered, only no longer filed");
        _ = this.Stored<ShoppingItemPrice>().Should().ContainSingle("what it cost is kept");
    }

    [Fact]
    public async Task HandleAsync_LeavesItemsInOtherAislesWhereTheyAre()
    {
        _ = this.Database.Seed(
            BuildMemory(120, this.Ours, "Milk", BuildCategory(110, this.Ours, "Dairy")),
            BuildMemory(121, this.Ours, "Peas", BuildCategory(111, this.Ours, "Frozen")));

        await this.HandleAsync(110);

        _ = this.FiledUnder(111).Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WhenTheAisleBelongsToAnotherHousehold_PresentsNotFoundAndKeepsItsItemsFiled()
    {
        _ = this.Database.Seed(
            BuildCategory(110, this.Ours, "Dairy"),
            BuildMemory(920, this.Theirs, "Milk", BuildCategory(910, this.Theirs, "Dairy")));

        await this.HandleAsync(910);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ShoppingCategory>().Should().HaveCount(2);
        _ = this.FiledUnder(910).Should().Be(1);
    }

    #endregion Methods

}
