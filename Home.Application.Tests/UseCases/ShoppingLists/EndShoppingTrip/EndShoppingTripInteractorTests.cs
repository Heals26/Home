using FluentAssertions;
using Home.Application.Infrastructure.ShoppingLists;
using Home.Application.Services.EntityLogic.ShoppingLists;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingLists.EndShoppingTrip;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Home.WebApi.Presenters.ShoppingLists.EndShoppingTrip;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.ShoppingLists.EndShoppingTrip;

/// <summary>
/// Done (16 Sep). The shop ends for every phone shopping with the list.
/// </summary>
public class EndShoppingTripInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<IAuditLogic<ShoppingList>> m_AuditLogic = new();
    private readonly EndShoppingTripPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Properties

    private static DateTime NowUTC
        => TestServiceFactory.DefaultNow.UtcDateTime;

    #endregion Properties

    #region Methods

    /// <summary>
    /// A list with one trip on it, numbered thirty past the list.
    /// </summary>
    private static ShoppingList BuildList(long shoppingListID, Household household, DateTime? endedOnUTC = null)
    {
        var _List = new ShoppingList()
        {
            Household = household,
            Name = $"List {shoppingListID}",
            ShoppingListID = shoppingListID
        };

        _List.Trips =
        [
            new ShoppingTrip()
            {
                EndedOnUTC = endedOnUTC,
                LastActivityOnUTC = NowUTC.AddMinutes(-5),
                ShoppingList = _List,
                ShoppingTripID = shoppingListID + 30,
                StartedOnUTC = NowUTC.AddMinutes(-45)
            }
        ];

        return _List;
    }

    private Task HandleAsync(long shoppingListID)
    {
        var _Services = this.Services(out var _Context);

        return new EndShoppingTripInteractor().HandleAsync(
            new EndShoppingTripInputPort(shoppingListID),
            this.m_Presenter,
            _Services
                .With(this.m_AuditLogic.Object)
                .With<IShoppingTripLogic>(new ShoppingTripLogic(_Context, _Services.Time))
                .Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_EndsTheShopGoingOnWithTheList()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours));

        await this.HandleAsync(120);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingTrip>().Single().EndedOnUTC.Should().Be(NowUTC);
        this.m_AuditLogic.Verify(a => a.UpdateAudit(It.IsAny<ShoppingList>(), "finished shopping with 'List 120'"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTheShopIsAlreadyFinished_ChangesNothing()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours, endedOnUTC: NowUTC.AddMinutes(-1)));

        await this.HandleAsync(120);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>("the other phone pressed Done first");
        _ = this.Stored<ShoppingTrip>().Single().EndedOnUTC.Should().Be(NowUTC.AddMinutes(-1));
        this.m_AuditLogic.Verify(a => a.UpdateAudit(It.IsAny<ShoppingList>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_LeavesAShopWithAnotherListGoing()
    {
        _ = this.Database.Seed(BuildList(120, this.Ours), BuildList(121, this.Ours));

        await this.HandleAsync(120);

        _ = this.Stored<ShoppingTrip>().Single(t => t.ShoppingTripID == 151).EndedOnUTC.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenTheListBelongsToAnotherHousehold_TouchesNothing()
    {
        _ = this.Database.Seed(BuildList(920, this.Theirs));

        await this.HandleAsync(920);

        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
        _ = this.Stored<ShoppingTrip>().Single().EndedOnUTC.Should().BeNull();
        this.m_AuditLogic.Verify(a => a.UpdateAudit(It.IsAny<ShoppingList>(), It.IsAny<string>()), Times.Never);
    }

    #endregion Methods

}
