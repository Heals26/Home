using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingListItems.DeleteShoppingListItem;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ShoppingListItems.DeleteShoppingListItem;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.ShoppingListItems.DeleteShoppingListItem;

/// <summary>
/// Taking one thing off a shopping list.
/// </summary>
public class DeleteShoppingListItemInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteShoppingListItemPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ShoppingList BuildList(long shoppingListID, Household household, params long[] itemIDs)
    {
        var _List = new ShoppingList()
        {
            Household = household,
            Name = $"List {shoppingListID}",
            ShoppingListID = shoppingListID
        };

        _List.Items =
        [
            .. itemIDs.Select((id, index) => new ShoppingListItem()
            {
                Name = $"Item {id}",
                Sequence = index + 1,
                ShoppingList = _List,
                ShoppingListItemID = id
            })
        ];

        return _List;
    }

    private Task HandleAsync(long shoppingListItemID)
        => new DeleteShoppingListItemInteractor().HandleAsync(
            new DeleteShoppingListItemInputPort(shoppingListItemID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesOnlyThatItem()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, 130, 131));

        await this.HandleAsync(130);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingListItem>().Select(i => i.ShoppingListItemID).Should().Equal([131]);
        _ = this.Stored<ShoppingList>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WhenTheItemBelongsToAnotherHousehold_PresentsNotFoundAndKeepsIt()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, 130), BuildList(920, this.Theirs, 930));

        await this.HandleAsync(930);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ShoppingListItem>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchItemExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, 130));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
