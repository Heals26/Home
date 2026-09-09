using FluentAssertions;
using Home.Application.Infrastructure.Calendar;
using Home.Application.Services.Calendar;
using Home.Application.Services.EntityLogic.Calendar;
using Home.Application.Tests.Infrastructure;
using Home.Application.Tests.UseCases.Calendar;
using Home.Application.UseCases.CalendarSubscriptions.RefreshCalendarSubscription;
using Home.Domain.Entities;
using Home.WebApi.Presenters.CalendarSubscriptions.RefreshCalendarSubscription;
using Moq;

namespace Home.Application.Tests.UseCases.CalendarSubscriptions.RefreshCalendarSubscription;

/// <summary>
/// Re-reading one feed. A refresh replaces everything that feed put there, so the two things worth
/// pinning are that it takes only its own rows and that a feed which cannot be reached leaves the
/// last good copy on screen.
/// </summary>
public class RefreshCalendarSubscriptionInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<ICalendarFeedService> m_FeedService = new();
    private readonly RefreshCalendarSubscriptionPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static CalendarSubscription Subscription(long calendarSubscriptionID, Household household, string name)
        => new()
        {
            CalendarSubscriptionID = calendarSubscriptionID,
            Household = household,
            LastFetchedUTC = new DateTime(2026, 9, 1, 6, 0, 0, DateTimeKind.Utc),
            Name = name,
            Url = $"https://{name.ToLowerInvariant()}.test/feed.ics"
        };

    private static CalendarEvent FeedEvent(long calendarEventID, Household household, CalendarSubscription subscription, string title)
    {
        var _Event = CalendarTestData.AllDayEvent(calendarEventID, household, title, new DateOnly(2026, 9, 15));

        _Event.Subscription = subscription;

        return _Event;
    }

    private static CalendarFeed FeedOf(params string[] titles)
        => new("School",
            [.. titles.Select((t, i) => new CalendarFeedOccurrence(
                $"uid-{i}", t, null, true, new DateOnly(2026, 9, 20 + i), new DateOnly(2026, 9, 20 + i), null, null))]);

    private async Task HandleAsync(long calendarSubscriptionID, CalendarFeed? feed)
    {
        _ = this.m_FeedService
            .Setup(f => f.ReadAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(feed);

        var _Services = this.Services(out var _Context);

        _ = _Services.With<ICalendarSubscriptionLogic>(new CalendarSubscriptionLogic(this.m_FeedService.Object, _Context, _Services.Time));

        await new RefreshCalendarSubscriptionInteractor().HandleAsync(
            new RefreshCalendarSubscriptionInputPort(calendarSubscriptionID),
            this.m_Presenter,
            _Services.Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_ReplacesWhatTheFeedPutThereWithWhatItSaysNow()
    {
        var _Subscription = Subscription(180, this.Ours, "School");

        _ = this.Database.Seed(_Subscription, FeedEvent(150, this.Ours, _Subscription, "Old assembly"));

        await this.HandleAsync(180, FeedOf("Sports day", "Book fair"));

        _ = this.Stored<CalendarEvent>().Select(e => e.Title).OrderBy(t => t).Should().Equal(["Book fair", "Sports day"]);
        _ = this.Stored<CalendarSubscription>().Single().LastFetchedUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
        _ = this.Stored<CalendarSubscription>().Single().LastError.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_TakesOnlyItsOwnRowsAndLeavesTheHouseholdsOwnEvents()
    {
        var _Subscription = Subscription(181, this.Ours, "School");
        var _Other = Subscription(182, this.Ours, "Swim");

        _ = this.Database.Seed(
            _Subscription,
            _Other,
            FeedEvent(151, this.Ours, _Subscription, "Old assembly"),
            FeedEvent(152, this.Ours, _Other, "Swim lesson"),
            CalendarTestData.AllDayEvent(153, this.Ours, "Bin day", new DateOnly(2026, 9, 15)));

        await this.HandleAsync(181, FeedOf("Sports day"));

        _ = this.Stored<CalendarEvent>().Select(e => e.Title).OrderBy(t => t).Should().Equal(
            ["Bin day", "Sports day", "Swim lesson"],
            "a refresh replaces one feed's rows, not the calendar");
    }

    [Fact]
    public async Task HandleAsync_KeepsTheLastGoodCopyWhenTheFeedCannotBeReached()
    {
        var _Subscription = Subscription(183, this.Ours, "School");

        _ = this.Database.Seed(_Subscription, FeedEvent(154, this.Ours, _Subscription, "Assembly"));

        await this.HandleAsync(183, null);

        _ = this.Stored<CalendarEvent>().Select(e => e.Title).Should().Equal(["Assembly"]);
        _ = this.Stored<CalendarSubscription>().Single().LastError.Should().NotBeNullOrEmpty();
        _ = this.Stored<CalendarSubscription>().Single().LastFetchedUTC.Should().Be(
            new DateTime(2026, 9, 1, 6, 0, 0, DateTimeKind.Utc),
            "a failed read is not a read");
    }

    [Fact]
    public async Task HandleAsync_WillNotRefreshAnotherHouseholdsFeed()
    {
        var _Theirs = Subscription(980, this.Theirs, "Theirs");

        _ = this.Database.Seed(_Theirs, FeedEvent(950, this.Theirs, _Theirs, "Their assembly"));

        await this.HandleAsync(980, FeedOf("Injected"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<CalendarEvent>().Select(e => e.Title).Should().Equal(["Their assembly"]);
    }

    [Fact]
    public async Task HandleAsync_PresentsNotFoundForASubscriptionThatDoesNotExist()
    {
        await this.HandleAsync(404, FeedOf("Sports day"));

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
