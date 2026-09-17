using FluentAssertions;
using Home.Application.Infrastructure.Undo;
using Home.Application.Services.Undo;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.ShoppingListItems.DeleteShoppingListItem;
using Home.Application.UseCases.Users.DeleteUser;
using Home.Domain.Entities;
using Home.Persistence.Undo;
using Home.WebApi.Presenters.ShoppingListItems.DeleteShoppingListItem;
using Home.WebApi.Presenters.Users.DeleteUser;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Tests.Infrastructure.Undo;

/// <summary>
/// The held-back deletes being carried out once no undo can reach them (17 Sep). Until then a held-back
/// row is only hidden, and afterwards it is gone for good.
/// </summary>
public class UndoPurgeTests : InteractorTest
{

    #region Fields

    private readonly Guid m_Token = Guid.NewGuid();

    #endregion Fields

    #region Properties

    private static DateTime NowUTC
        => TestServiceFactory.DefaultNow.UtcDateTime;

    #endregion Properties

    #region Methods

    private Task<int> PurgeAsync(DateTime beforeUTC)
    {
        var _Services = this.Services(out var _Context);

        return ((IUndoStore)new UndoStore(_Context, _Services.Time)).PurgeAsync(beforeUTC, CancellationToken.None);
    }

    /// <summary>
    /// Every row of the kind, held back or not, because a purge is judged by what is left in the
    /// store rather than by what a query shows.
    /// </summary>
    private int StoredIncludingDeleted<TEntity>() where TEntity : class
        => ((DbContext)this.Database.Read()).Set<TEntity>().IgnoreQueryFilters().Count();

    private UndoScope WithUndo()
        => new() { HouseholdID = OurHouseholdID, Token = this.m_Token };

    [Fact]
    public async Task PurgeAsync_DeletesForGoodWhatNoUndoCanReach()
    {
        var _List = new ShoppingList() { Household = this.Ours, Name = "This week", ShoppingListID = 120 };

        _List.Items = [new ShoppingListItem() { Name = "Milk", ShoppingList = _List, ShoppingListItemID = 130 }];

        _ = this.Database.Seed(_List);

        await new DeleteShoppingListItemInteractor().HandleAsync(
            new DeleteShoppingListItemInputPort(130),
            new DeleteShoppingListItemPresenter(Mapper),
            this.Services(out _, this.WithUndo()).Build(),
            CancellationToken.None);

        var _Failures = await this.PurgeAsync(NowUTC.AddSeconds(1));

        _ = _Failures.Should().Be(0);
        _ = this.StoredIncludingDeleted<ShoppingListItem>().Should().Be(0);
        _ = this.StoredIncludingDeleted<UndoableAction>().Should().Be(0, "the record of what the request changed goes with it");
    }

    [Fact]
    public async Task PurgeAsync_KeepsWhatAnUndoCanStillReach()
    {
        var _List = new ShoppingList() { Household = this.Ours, Name = "This week", ShoppingListID = 120 };

        _List.Items = [new ShoppingListItem() { Name = "Milk", ShoppingList = _List, ShoppingListItemID = 130 }];

        _ = this.Database.Seed(_List);

        await new DeleteShoppingListItemInteractor().HandleAsync(
            new DeleteShoppingListItemInputPort(130),
            new DeleteShoppingListItemPresenter(Mapper),
            this.Services(out _, this.WithUndo()).Build(),
            CancellationToken.None);

        _ = await this.PurgeAsync(NowUTC.AddSeconds(-1));

        _ = this.StoredIncludingDeleted<ShoppingListItem>().Should().Be(1);
        _ = this.StoredIncludingDeleted<UndoableAction>().Should().Be(1);
    }

    [Fact]
    public async Task PurgeAsync_DeletesARowThatPointsAtAnotherBeforeTheRowItPointsAt()
    {
        var _Bo = new User() { UserID = 102, FirstName = "Bo", Household = this.Ours, LastName = "Member" };

        _ = this.Database.Seed(new CalendarEvent()
        {
            CalendarEventID = 160,
            EndDate = new DateOnly(2026, 9, 10),
            Household = this.Ours,
            IsAllDay = true,
            Members = [new CalendarEventMember() { User = _Bo }],
            StartDate = new DateOnly(2026, 9, 10),
            Title = "Swimming"
        });

        await new DeleteUserInteractor().HandleAsync(
            new DeleteUserInputPort(102),
            new DeleteUserPresenter(Mapper),
            this.Services(out _, this.WithUndo()).Build(),
            CancellationToken.None);

        var _Failures = await this.PurgeAsync(NowUTC.AddSeconds(1));

        _ = _Failures.Should().Be(0);
        _ = this.StoredIncludingDeleted<CalendarEventMember>().Should().Be(0);
        _ = this.StoredIncludingDeleted<User>().Should().Be(0);
    }

    #endregion Methods

}
