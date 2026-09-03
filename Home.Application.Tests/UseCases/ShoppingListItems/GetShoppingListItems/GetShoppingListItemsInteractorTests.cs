using FluentAssertions;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingListItems.GetShoppingListItems;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ShoppingListItems.GetShoppingListItems;
using Home.WebApi.UseCases.ShoppingListItems.GetShoppingListItems;

namespace Home.Application.Tests.UseCases.ShoppingListItems.GetShoppingListItems;

/// <summary>
/// The shopping list as the shopper reads it. The only read that checks the household on one
/// query and then fetches through <see cref="IShoppingListLogic"/> on another, so the real logic
/// is wired up here rather than mocked — the split is exactly where an isolation hole could open,
/// since the second query filters on the list ID alone.
/// </summary>
public class GetShoppingListItemsInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetShoppingListItemsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static ShoppingList BuildList(long shoppingListID, Household household, params ShoppingListItem[] items)
    {
        var _List = new ShoppingList()
        {
            Household = household,
            Name = $"List {shoppingListID}",
            ShoppingListID = shoppingListID
        };

        foreach (var _Item in items)
            _Item.ShoppingList = _List;

        _List.Items = items;

        return _List;
    }

    private Task HandleAsync(long shoppingListID)
    {
        var _Context = this.Database.Read();

        return new GetShoppingListItemsInteractor().HandleAsync(
            new GetShoppingListItemsInputPort(shoppingListID),
            this.m_Presenter,
            new TestServiceFactory()
                .With(_Context)
                .With(this.AuthorisationService.Object)
                .With<IShoppingListLogic>(new ShoppingListLogic(_Context))
                .Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_ReturnsTheListInTheOrderItWasPutIn()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours,
            new ShoppingListItem() { Name = "Bread", Sequence = 2, ShoppingListItemID = 131 },
            new ShoppingListItem() { Name = "Milk", Sequence = 1, ShoppingListItemID = 130 },
            new ShoppingListItem() { InBasket = true, Name = "Eggs", Sequence = 3, ShoppingListItemID = 132 }));

        await this.HandleAsync(120);

        var _Items = Ok<GetShoppingListItemsApiResponse>(this.m_Presenter).Items;

        _ = _Items.Select(i => i.Name).Should().Equal(["Milk", "Bread", "Eggs"]);
        _ = _Items.Single(i => i.Name == "Eggs").InBasket.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenTheListBelongsToAnotherHousehold_PresentsNotFoundRatherThanItsContents()
    {
        _ = this.Database.Seed(
            BuildList(120, this.Ours, new ShoppingListItem() { Name = "Milk", Sequence = 1, ShoppingListItemID = 130 }),
            BuildList(920, this.Theirs, new ShoppingListItem() { Name = "Caviar", Sequence = 1, ShoppingListItemID = 930 }));

        await this.HandleAsync(920);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSuchListExists_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, new ShoppingListItem() { Name = "Milk", Sequence = 1, ShoppingListItemID = 130 }));

        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_WhenTheListIsEmpty_PresentsAnEmptyListRatherThanNotFound()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours));

        await this.HandleAsync(120);

        _ = Ok<GetShoppingListItemsApiResponse>(this.m_Presenter).Items.Should().BeEmpty();
    }

    #endregion Methods

}
