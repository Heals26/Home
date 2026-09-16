using FluentAssertions;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Services.Persistence;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingLists.StartShoppingTrip;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.WebApi.Presenters.ShoppingLists.StartShoppingTrip;
using Home.WebApi.UseCases.ShoppingLists.StartShoppingTrip;
using Moq;

namespace Home.Application.Tests.UseCases.ShoppingLists.StartShoppingTrip;

/// <summary>
/// Switching shopping mode on (16 Sep). A list has one shop going on at a time, however many phones
/// join it.
/// </summary>
public class StartShoppingTripInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<ShoppingList>> m_AuditLogic = new();
    private readonly StartShoppingTripPresenter m_Presenter = new(Mapper);

    /// <summary>
    /// How long after <see cref="TestServiceFactory.DefaultNow"/> the request happens.
    /// </summary>
    private TimeSpan m_Elapsed;

    #endregion Fields

    #region Properties

    private static DateTime NowUTC
        => TestServiceFactory.DefaultNow.UtcDateTime;

    #endregion Properties

    #region Methods

    private static ShoppingList BuildList(long shoppingListID, Household household)
        => new()
        {
            Household = household,
            Name = $"List {shoppingListID}",
            ShoppingListID = shoppingListID
        };

    private Task HandleAsync(long shoppingListID)
    {
        var _Services = this.Services(out var _Context);

        _Services.Time.Advance(this.m_Elapsed);

        return new StartShoppingTripInteractor().HandleAsync(
            new StartShoppingTripInputPort(shoppingListID),
            this.m_Presenter,
            _Services
                .With(this.m_AuditLogic.Object)
                .With<IShoppingTripLogic>(new ShoppingTripLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    private static ShoppingTrip Trip(long shoppingTripID, ShoppingList shoppingList, DateTime lastActivityOnUTC, bool ended = false)
        => new()
        {
            EndedOnUTC = ended ? lastActivityOnUTC : null,
            LastActivityOnUTC = lastActivityOnUTC,
            ShoppingList = shoppingList,
            ShoppingTripID = shoppingTripID,
            StartedOnUTC = lastActivityOnUTC
        };

    [Fact]
    public async Task HandleAsync_StartsAShopOnTheList()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours));

        await this.HandleAsync(120);

        var _Trip = this.Stored<ShoppingTrip>().Single(t => t.ShoppingList.ShoppingListID == 120);

        _ = Ok<StartShoppingTripApiResponse>(this.m_Presenter).ShoppingTripID.Should().Be(_Trip.ShoppingTripID);
        _ = _Trip.EndedOnUTC.Should().BeNull();
        _ = _Trip.LastActivityOnUTC.Should().Be(NowUTC);
        _ = _Trip.StartedOnUTC.Should().Be(NowUTC);
        this.m_AuditLogic.Verify(a => a.UpdateAudit(It.IsAny<ShoppingList>(), "started shopping with 'List 120'"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_JoinsTheShopAlreadyGoingOnRatherThanStartingASecond()
    {
        var _List = BuildList(120, this.Ours);

        _List.Trips = [Trip(150, _List, NowUTC)];

        _ = this.Database.Seed(_List);

        this.m_Elapsed = TimeSpan.FromMinutes(15);
        await this.HandleAsync(120);

        _ = Ok<StartShoppingTripApiResponse>(this.m_Presenter).ShoppingTripID.Should().Be(150);
        _ = this.Stored<ShoppingTrip>().Should().ContainSingle("two people who split up at the door are one shop");
        _ = this.Stored<ShoppingTrip>().Single().LastActivityOnUTC.Should().Be(NowUTC.AddMinutes(15));
        this.m_AuditLogic.Verify(a => a.UpdateAudit(It.IsAny<ShoppingList>(), "joined in shopping with 'List 120'"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_StartsAFreshShopOnceTheLastOneIsFinished()
    {
        var _List = BuildList(120, this.Ours);

        _List.Trips = [Trip(150, _List, NowUTC.AddMinutes(-10), ended: true)];

        _ = this.Database.Seed(_List);

        await this.HandleAsync(120);

        _ = Ok<StartShoppingTripApiResponse>(this.m_Presenter).ShoppingTripID.Should().NotBe(150);
        _ = this.Stored<ShoppingTrip>().Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_StartsAFreshShopOnceTheLastOneHasGoneQuiet()
    {
        var _List = BuildList(120, this.Ours);

        _List.Trips = [Trip(150, _List, NowUTC - ShoppingTripLogic.QuietSpell - TimeSpan.FromMinutes(1))];

        _ = this.Database.Seed(_List);

        await this.HandleAsync(120);

        _ = Ok<StartShoppingTripApiResponse>(this.m_Presenter).ShoppingTripID.Should().NotBe(150);
    }

    [Fact]
    public async Task HandleAsync_NeverJoinsAShopGoingOnWithAnotherList()
    {
        var _Other = BuildList(121, this.Ours);

        _Other.Trips = [Trip(150, _Other, NowUTC)];

        _ = this.Database.Seed(BuildList(120, this.Ours), _Other);

        await this.HandleAsync(120);

        _ = Ok<StartShoppingTripApiResponse>(this.m_Presenter).ShoppingTripID.Should().NotBe(150);
    }

    [Fact]
    public async Task HandleAsync_WhenAnotherPhoneStartsAShopAtTheSameMoment_BothKeepTheOneSavedFirst()
    {
        var _List = BuildList(120, this.Ours);

        // Last week's shop moves the numbering on, so the trip this request adds is numbered after the
        // other phone's, as SQL Server numbers whichever is saved first.
        _List.Trips = [Trip(150, _List, NowUTC.AddDays(-7), ended: true)];

        _ = this.Database.Seed(_List);

        var _Services = this.Services(out var _Context);
        var _Raced = new RacedPersistenceContext(_Context, () => this.Database.Seed(Trip(145, _List, NowUTC)), clashes: false);

        await new StartShoppingTripInteractor().HandleAsync(
            new StartShoppingTripInputPort(120),
            this.m_Presenter,
            _Services
                .With<IPersistenceContext>(_Raced)
                .With<IShoppingTripLogic>(new ShoppingTripLogic(_Raced, _Services.Time))
                .Build(),
            CancellationToken.None);

        _ = Ok<StartShoppingTripApiResponse>(this.m_Presenter).ShoppingTripID.Should().Be(145);
        _ = this.Stored<ShoppingTrip>().Count(t => t.EndedOnUTC == null).Should().Be(1, "a second trip left behind would split the shop in two");
    }

    [Fact]
    public async Task HandleAsync_WhenTheListBelongsToAnotherHousehold_PresentsNotFound()
    {
        _ = this.Database.Seed(BuildList(920, this.Theirs));

        await this.HandleAsync(920);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<ShoppingTrip>().Should().BeEmpty();
    }

    #endregion Methods

}
