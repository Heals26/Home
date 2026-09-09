using FluentAssertions;
using Home.Application.Infrastructure.Calendar;
using Home.Application.Services.Calendar;
using Home.Application.Services.EntityLogic.Calendar;
using Home.Application.Tests.Infrastructure;
using Home.Application.Tests.UseCases.Calendar;
using Home.Application.UseCases.CalendarSubscriptions.RefreshAllCalendarSubscriptions;
using Home.Domain.Entities;
using Home.WebApi.Presenters.CalendarSubscriptions.RefreshAllCalendarSubscriptions;
using Moq;

namespace Home.Application.Tests.UseCases.CalendarSubscriptions.RefreshAllCalendarSubscriptions;

/// <summary>
/// The half-hourly runner, and the one slice in the calendar that crosses households on purpose:
/// nobody is signed in, so it reads every feed there is. That makes "each household's events stay
/// its own" the thing worth pinning, because the usual authorisation guard is not there to do it.
/// </summary>
public class RefreshAllCalendarSubscriptionsInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<ICalendarFeedService> m_FeedService = new();
    private readonly RefreshAllCalendarSubscriptionsPresenter m_Presenter = new(Mapper);

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

    private static CalendarFeed FeedNamed(string title)
        => new("Feed", [new CalendarFeedOccurrence("uid-1", title, null, true, new DateOnly(2026, 9, 20), new DateOnly(2026, 9, 20), null, null)]);

    /// <summary>
    /// Each feed answers with an event named after its own address, so a row landing under the
    /// wrong household names the feed that leaked it.
    /// </summary>
    private void FeedsAnswerByUrl(params string[] unreachableUrls)
        => this.m_FeedService
            .Setup(f => f.ReadAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string url, DateTime _, DateTime __, CancellationToken ___)
                => unreachableUrls.Contains(url) ? null : FeedNamed($"From {new Uri(url).Host}"));

    private async Task HandleAsync()
    {
        var _Services = this.Services(out var _Context);

        _ = _Services.With<ICalendarSubscriptionLogic>(new CalendarSubscriptionLogic(this.m_FeedService.Object, _Context, _Services.Time));

        await new RefreshAllCalendarSubscriptionsInteractor().HandleAsync(
            new RefreshAllCalendarSubscriptionsInputPort(),
            this.m_Presenter,
            _Services.Build(),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_RefreshesEveryHouseholdsFeedBecauseNobodyIsSignedIn()
    {
        _ = this.Database.Seed(
            Subscription(180, this.Ours, "Ours"),
            Subscription(980, this.Theirs, "Theirs"));

        this.FeedsAnswerByUrl();

        await this.HandleAsync();

        _ = this.Stored<CalendarEvent>().Should().HaveCount(2, "the runner is the one slice that is not scoped to a household");
        _ = this.m_Presenter.RefreshedHouseholdIDs.Should().BeEquivalentTo([OurHouseholdID, TheirHouseholdID]);
    }

    [Fact]
    public async Task HandleAsync_KeepsEachFeedsEventsUnderTheHouseholdThatSubscribed()
    {
        _ = this.Database.Seed(
            Subscription(181, this.Ours, "Ours"),
            Subscription(981, this.Theirs, "Theirs"));

        this.FeedsAnswerByUrl();

        await this.HandleAsync();

        var _Ours = this.Stored<CalendarEvent>().Where(e => e.Household.HouseholdID == OurHouseholdID).Select(e => e.Title).ToList();
        var _Theirs = this.Stored<CalendarEvent>().Where(e => e.Household.HouseholdID == TheirHouseholdID).Select(e => e.Title).ToList();

        _ = _Ours.Should().Equal(["From ours.test"]);
        _ = _Theirs.Should().Equal(["From theirs.test"]);
    }

    [Fact]
    public async Task HandleAsync_ReplacesOnlyTheRowsEachFeedPutThere()
    {
        var _Ours = Subscription(182, this.Ours, "Ours");

        _ = this.Database.Seed(
            _Ours,
            FeedEvent(150, this.Ours, _Ours, "Stale"),
            CalendarTestData.AllDayEvent(151, this.Ours, "Bin day", new DateOnly(2026, 9, 15)));

        this.FeedsAnswerByUrl();

        await this.HandleAsync();

        _ = this.Stored<CalendarEvent>().Select(e => e.Title).OrderBy(t => t).Should().Equal(["Bin day", "From ours.test"]);
    }

    [Fact]
    public async Task HandleAsync_CountsTheOnesItCouldNotReachAndStillRefreshesTheRest()
    {
        _ = this.Database.Seed(
            Subscription(183, this.Ours, "Ours"),
            Subscription(983, this.Theirs, "Theirs"));

        this.FeedsAnswerByUrl("https://theirs.test/feed.ics");

        await this.HandleAsync();

        _ = this.m_Presenter.UnreachableSubscriptions.Should().Be(1);
        _ = this.m_Presenter.RefreshedHouseholdIDs.Should().Equal([OurHouseholdID], "an unreachable feed's household has nothing new to be told about");
        _ = this.Stored<CalendarSubscription>().Single(s => s.CalendarSubscriptionID == 983).LastError.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HandleAsync_NamesAHouseholdOnceHoweverManyFeedsItHas()
    {
        _ = this.Database.Seed(
            Subscription(184, this.Ours, "School"),
            Subscription(185, this.Ours, "Swim"));

        this.FeedsAnswerByUrl();

        await this.HandleAsync();

        _ = this.m_Presenter.RefreshedHouseholdIDs.Should().Equal([OurHouseholdID]);
    }

    [Fact]
    public async Task HandleAsync_DoesNothingQuietlyWhenNobodySubscribesToAnything()
    {
        this.FeedsAnswerByUrl();

        await this.HandleAsync();

        _ = this.m_Presenter.RefreshedHouseholdIDs.Should().BeEmpty();
        _ = this.m_Presenter.UnreachableSubscriptions.Should().Be(0);
    }

    #endregion Methods

}
