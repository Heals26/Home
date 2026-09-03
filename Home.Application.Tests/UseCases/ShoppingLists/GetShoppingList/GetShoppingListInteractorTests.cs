using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingLists.GetShoppingList;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ShoppingLists.GetShoppingList;
using Home.WebApi.UseCases.ShoppingLists.GetShoppingList;

namespace Home.Application.Tests.UseCases.ShoppingLists.GetShoppingList;

/// <summary>
/// One shopping list with everything on it. The items are the whole payload, and they arrive only
/// because the query names them — the 17 Aug outage here was a mapping gap, and this is the other
/// half of the same failure.
/// </summary>
public class GetShoppingListInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetShoppingListPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ShoppingList BuildList(long shoppingListID, Household household, string name, params string[] items)
    {
        var _List = new ShoppingList()
        {
            Household = household,
            Name = name,
            ShoppingListID = shoppingListID
        };

        _List.Items =
        [
            .. items.Select((n, index) => new ShoppingListItem()
            {
                Amount = index + 1,
                Name = n,
                Sequence = index + 1,
                ShoppingList = _List,
                ShoppingListItemID = shoppingListID + index + 1
            })
        ];

        return _List;
    }

    private Task HandleAsync(long shoppingListID)
        => new GetShoppingListInteractor().HandleAsync(
            new GetShoppingListInputPort(shoppingListID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_BringsBackEverythingOnTheList()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, "This week", "Milk", "Bread"));

        await this.HandleAsync(120);

        var _Response = Ok<GetShoppingListApiResponse>(this.m_Presenter);

        _ = _Response.Name.Should().Be("This week");
        _ = _Response.Items.Select(i => i.Name).Should().BeEquivalentTo(
            ["Milk", "Bread"],
            "an unprojected item collection hands the shopper an empty list");
    }

    [Fact]
    public async Task HandleAsync_WhenTheListBelongsToAnotherHousehold_PresentsNotFound()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, "This week", "Milk"),
            BuildList(920, this.Theirs, "Their week", "Caviar"));

        await this.HandleAsync(920);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchListExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, "This week", "Milk"));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
