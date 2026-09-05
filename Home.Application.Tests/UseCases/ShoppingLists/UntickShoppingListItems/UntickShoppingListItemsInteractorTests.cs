using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingLists.UntickShoppingListItems;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.WebApi.Presenters.ShoppingLists.UntickShoppingListItems;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.ShoppingLists.UntickShoppingListItems;

/// <summary>
/// Putting a standing list back to the start of the week. Everything comes out of the basket and
/// nothing is deleted, which is what separates this from closing off a shop.
/// </summary>
public class UntickShoppingListItemsInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<ShoppingList>> m_AuditLogic = new();
    private readonly UntickShoppingListItemsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ShoppingList BuildList(long shoppingListID, Household household, params (long ItemID, string Name, bool InBasket)[] items)
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
                InBasket = i.InBasket,
                Name = i.Name,
                Sequence = index + 1,
                ShoppingList = _List,
                ShoppingListItemID = i.ItemID
            })
        ];

        return _List;
    }

    private Task HandleAsync(long shoppingListID)
        => new UntickShoppingListItemsInteractor().HandleAsync(
            new UntickShoppingListItemsInputPort(shoppingListID),
            this.m_Presenter,
            this.Services().With(this.m_AuditLogic.Object).Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_TakesEverythingBackOutOfTheBasketAndKeepsItAll()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours,
            (130, "Milk", true),
            (131, "Bread", false),
            (132, "Eggs", true)));

        await this.HandleAsync(120);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingListItem>().Should().HaveCount(3, "nothing is deleted, which is the whole difference from closing off a shop");
        _ = this.Stored<ShoppingListItem>().Should().OnlyContain(i => !i.InBasket);
    }

    [Fact]
    public async Task HandleAsync_WhenNothingIsTicked_LeavesTheListAlone()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", false)));

        await this.HandleAsync(120);

        _ = this.Stored<ShoppingListItem>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenTheListBelongsToAnotherHousehold_TouchesNothing()
    {
        _ = this.Database.Seed(BuildList(920, this.Theirs, (930, "Caviar", true)));

        await this.HandleAsync(920);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingListItem>().Single().InBasket.Should().BeTrue();
        this.m_AuditLogic.Verify(a => a.UpdateAudit(It.IsAny<ShoppingList>()), Times.Never);
    }

    #endregion Methods

}
