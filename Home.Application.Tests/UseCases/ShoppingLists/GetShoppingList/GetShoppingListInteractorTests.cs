using FluentAssertions;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingLists.GetShoppingList;
using Home.Domain.Entities;
using Home.WebApi.Presenters.ShoppingLists.GetShoppingList;
using Home.WebApi.UseCases.ShoppingLists.GetShoppingList;

namespace Home.Application.Tests.UseCases.ShoppingLists.GetShoppingList;

/// <summary>
/// One shopping list with everything on it. The items are the whole payload, and they arrive only
/// because the query names them. The 17 Aug outage here was a mapping gap, and this is the other
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
    {
        var _Services = this.Services(out var _Context);

        return new GetShoppingListInteractor().HandleAsync(
            new GetShoppingListInputPort(shoppingListID),
            this.m_Presenter,
            _Services
                .With<IShoppingItemMemoryLogic>(new ShoppingItemMemoryLogic(_Context, _Services.Time))
                .With<IShoppingTripLogic>(new ShoppingTripLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    private static ShoppingTrip Trip(long shoppingTripID, ShoppingList shoppingList, TimeSpan quietFor, bool ended = false)
    {
        var _LastActivity = TestServiceFactory.DefaultNow.UtcDateTime - quietFor;

        return new()
        {
            EndedOnUTC = ended ? _LastActivity : null,
            LastActivityOnUTC = _LastActivity,
            ShoppingList = shoppingList,
            ShoppingTripID = shoppingTripID,
            StartedOnUTC = _LastActivity.AddMinutes(-30)
        };
    }

    [Fact]
    public async Task HandleAsync_SaysWhichShopIsGoingOnWithTheList()
    {
        var _List = BuildList(120, this.Ours, "This week", "Milk");

        _List.Trips =
        [
            Trip(150, _List, TimeSpan.FromDays(7), ended: true),
            Trip(151, _List, TimeSpan.FromDays(3)),
            Trip(152, _List, TimeSpan.FromMinutes(5))
        ];

        _ = this.Database.Seed(_List);

        await this.HandleAsync(120);

        _ = Ok<GetShoppingListApiResponse>(this.m_Presenter).ShoppingTripID.Should().Be(152);
    }

    [Fact]
    public async Task HandleAsync_WhenTheLastShopHasGoneQuiet_SaysNobodyIsShopping()
    {
        var _List = BuildList(120, this.Ours, "This week", "Milk");

        _List.Trips = [Trip(150, _List, ShoppingTripLogic.QuietSpell + TimeSpan.FromMinutes(1))];

        _ = this.Database.Seed(_List);

        await this.HandleAsync(120);

        _ = Ok<GetShoppingListApiResponse>(this.m_Presenter).ShoppingTripID.Should().BeNull("a shop nobody finished is over after two quiet hours");
    }

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
