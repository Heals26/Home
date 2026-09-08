using FluentAssertions;
using Home.Application.Infrastructure.Calendar;
using Home.Application.Services.Calendar;
using Home.Application.Services.EntityLogic.Calendar;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.CalendarSubscriptions.CreateCalendarSubscription;
using Home.Domain.Entities;
using Home.WebApi.Presenters.CalendarSubscriptions.CreateCalendarSubscription;
using Home.WebApi.UseCases.CalendarSubscriptions.CreateCalendarSubscription;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Home.Application.Tests.UseCases.CalendarSubscriptions.CreateCalendarSubscription;

/// <summary>
/// Adding a feed reads it straight away through the real refresh logic, with only the fetch itself
/// faked, so what lands in the table is what the runner will later keep replacing.
/// </summary>
public class CreateCalendarSubscriptionInteractorTests : InteractorTest
{

    #region Fields

    private readonly Mock<ICalendarFeedService> m_FeedService = new();
    private readonly CreateCalendarSubscriptionPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static CalendarFeed SchoolFeed()
        => new("Springfield Primary",
        [
            new CalendarFeedOccurrence("uid-1", "School photos", null, true, new DateOnly(2026, 9, 15), new DateOnly(2026, 9, 15), null, null),
            new CalendarFeedOccurrence("uid-2", "Assembly", "Hall", false, new DateOnly(2026, 9, 16), new DateOnly(2026, 9, 16), new TimeOnly(23, 30), new TimeOnly(23, 45))
        ]);

    private async Task HandleAsync(string name, string url, CalendarFeed? feed)
    {
        _ = this.m_FeedService
            .Setup(f => f.ReadAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(feed);

        var _Services = this.Services(out var _Context);

        _ = _Services.With<ICalendarSubscriptionLogic>(new CalendarSubscriptionLogic(this.m_FeedService.Object, _Context, _Services.Time));

        await new CreateCalendarSubscriptionInteractor().HandleAsync(
            new CreateCalendarSubscriptionInputPort(name, url),
            this.m_Presenter,
            _Services.Build(),
            CancellationToken.None);
    }

    private CreateCalendarSubscriptionApiResponse Created()
        => this.m_Presenter.Result.Should().BeOfType<CreatedResult>().Which
            .Value.Should().BeOfType<CreateCalendarSubscriptionApiResponse>().Which;

    [Fact]
    public async Task HandleAsync_StoresTheFeedsOccurrencesAsReadOnlyEventsInUtc()
    {
        await this.HandleAsync("School", "https://school.test/feed.ics", SchoolFeed());

        var _Events = this.Stored<CalendarEvent>().OrderBy(e => e.StartDate).ToList();

        _ = _Events.Should().HaveCount(2);
        _ = this.Stored<CalendarEvent>().Count(e => e.Subscription != null && e.Household.HouseholdID == OurHouseholdID).Should().Be(2);
        _ = _Events[0].IsAllDay.Should().BeTrue();
        _ = _Events[0].TimeZoneID.Should().BeNull();
        _ = _Events[1].TimeZoneID.Should().Be("UTC", "a feed's timed occurrences arrive as UTC instants");
        _ = _Events[1].StartTime.Should().Be(new TimeOnly(23, 30));
        _ = _Events[1].ExternalUID.Should().Be("uid-2");
        _ = this.Created().WasFetched.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_TakesTheFeedsOwnNameWhenNoneWasGiven()
    {
        await this.HandleAsync("  ", "https://school.test/feed.ics", SchoolFeed());

        var _Subscription = this.Stored<CalendarSubscription>().Single();

        _ = _Subscription.Name.Should().Be("Springfield Primary");
        _ = _Subscription.LastFetchedUTC.Should().Be(TestServiceFactory.DefaultNow.UtcDateTime);
        _ = _Subscription.LastError.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_TurnsAWebcalAddressIntoHttps()
    {
        await this.HandleAsync("School", "webcal://school.test/feed.ics", SchoolFeed());

        _ = this.Stored<CalendarSubscription>().Single().Url.Should().Be("https://school.test/feed.ics");
    }

    [Fact]
    public async Task HandleAsync_KeepsTheSubscriptionAndRecordsWhyWhenTheFeedCannotBeRead()
    {
        await this.HandleAsync("School", "https://school.test/feed.ics", null);

        var _Subscription = this.Stored<CalendarSubscription>().Single();

        _ = _Subscription.LastError.Should().NotBeNullOrEmpty();
        _ = _Subscription.LastFetchedUTC.Should().BeNull();
        _ = this.Stored<CalendarEvent>().Should().BeEmpty();
        _ = this.Created().WasFetched.Should().BeFalse();
    }

    #endregion Methods

}
