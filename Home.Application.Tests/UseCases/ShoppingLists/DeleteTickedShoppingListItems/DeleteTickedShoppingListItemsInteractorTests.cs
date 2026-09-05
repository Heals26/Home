using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingLists.DeleteTickedShoppingListItems;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.WebApi.Presenters.ShoppingLists.DeleteTickedShoppingListItems;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.ShoppingLists.DeleteTickedShoppingListItems;

/// <summary>
/// Closing off a shop. Everything in the basket goes in one call, because doing it line by line
/// from a phone in a supermarket is a round trip each and the list drains instead of emptying.
/// </summary>
public class DeleteTickedShoppingListItemsInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<ShoppingList>> m_AuditLogic = new();
    private readonly DeleteTickedShoppingListItemsPresenter m_Presenter = new(Mapper);

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
        => new DeleteTickedShoppingListItemsInteractor().HandleAsync(
            new DeleteTickedShoppingListItemsInputPort(shoppingListID),
            this.m_Presenter,
            this.Services().With(this.m_AuditLogic.Object).Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesEverythingInTheBasketAndLeavesTheRest()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours,
            (130, "Milk", true),
            (131, "Bread", false),
            (132, "Eggs", true)));

        await this.HandleAsync(120);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingListItem>().Select(i => i.Name).Should().Equal(["Bread"]);
    }

    [Fact]
    public async Task HandleAsync_WhenNothingIsTicked_LeavesTheListAlone()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", false)));

        await this.HandleAsync(120);

        _ = this.Stored<ShoppingListItem>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_KeepsTheListItself()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, (130, "Milk", true)));

        await this.HandleAsync(120);

        _ = this.Stored<ShoppingList>().Should().ContainSingle("closing off a shop empties the list, it does not delete it");
    }

    [Fact]
    public async Task HandleAsync_WhenTheListBelongsToAnotherHousehold_TouchesNothing()
    {
        _ = this.Database.Seed(BuildList(920, this.Theirs, (930, "Caviar", true)));

        await this.HandleAsync(920);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingListItem>().Should().ContainSingle();
        this.m_AuditLogic.Verify(a => a.UpdateAudit(It.IsAny<ShoppingList>()), Times.Never);
    }

    #endregion Methods

}
