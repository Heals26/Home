using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.Tests.UseCases.Calendar;
using Home.Application.UseCases.CalendarSubscriptions.DeleteCalendarSubscription;
using Home.Domain.Entities;
using Home.WebApi.Presenters.CalendarSubscriptions.DeleteCalendarSubscription;

namespace Home.Application.Tests.UseCases.CalendarSubscriptions.DeleteCalendarSubscription;

/// <summary>
/// Removing a feed. Its events cannot cascade from it, because the household already cascades to
/// both, so the interactor deletes them itself and this is what proves it does.
/// </summary>
public class DeleteCalendarSubscriptionInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteCalendarSubscriptionPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static CalendarSubscription Subscription(long calendarSubscriptionID, Household household, string name)
        => new()
        {
            CalendarSubscriptionID = calendarSubscriptionID,
            Household = household,
            Name = name,
            Url = $"https://{name.ToLowerInvariant()}.test/feed.ics"
        };

    private static CalendarEvent FeedEvent(long calendarEventID, Household household, CalendarSubscription subscription, string title)
    {
        var _Event = CalendarTestData.AllDayEvent(calendarEventID, household, title, new DateOnly(2026, 9, 15));

        _Event.Subscription = subscription;

        return _Event;
    }

    private Task HandleAsync(long calendarSubscriptionID)
        => new DeleteCalendarSubscriptionInteractor().HandleAsync(
            new DeleteCalendarSubscriptionInputPort(calendarSubscriptionID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    [Fact]
    public async Task HandleAsync_RemovesTheSubscriptionAndEveryEventItPutThere()
    {
        var _Subscription = Subscription(180, this.Ours, "School");

        _ = this.Database.Seed(
            _Subscription,
            FeedEvent(150, this.Ours, _Subscription, "School photos"),
            FeedEvent(151, this.Ours, _Subscription, "Assembly"));

        await this.HandleAsync(180);

        _ = this.Stored<CalendarSubscription>().Should().BeEmpty();
        _ = this.Stored<CalendarEvent>().Should().BeEmpty("a feed's events cannot cascade, so the interactor has to take them");
    }

    [Fact]
    public async Task HandleAsync_LeavesTheHouseholdsOwnEventsStanding()
    {
        var _Subscription = Subscription(181, this.Ours, "School");

        _ = this.Database.Seed(
            _Subscription,
            FeedEvent(152, this.Ours, _Subscription, "Assembly"),
            CalendarTestData.AllDayEvent(153, this.Ours, "Bin day", new DateOnly(2026, 9, 15)));

        await this.HandleAsync(181);

        _ = this.Stored<CalendarEvent>().Select(e => e.Title).Should().Equal(["Bin day"]);
    }

    [Fact]
    public async Task HandleAsync_LeavesAnotherHouseholdsFeedAloneAndSaysNotFound()
    {
        var _Theirs = Subscription(980, this.Theirs, "Theirs");

        _ = this.Database.Seed(_Theirs, FeedEvent(950, this.Theirs, _Theirs, "Their assembly"));

        await this.HandleAsync(980);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<CalendarSubscription>().Should().HaveCount(1);
        _ = this.Stored<CalendarEvent>().Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleAsync_PresentsNotFoundForASubscriptionThatDoesNotExist()
    {
        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
