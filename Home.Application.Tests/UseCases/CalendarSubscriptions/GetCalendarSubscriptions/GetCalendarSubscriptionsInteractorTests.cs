using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.CalendarSubscriptions.GetCalendarSubscriptions;
using Home.Domain.Entities;
using Home.WebApi.Presenters.CalendarSubscriptions.GetCalendarSubscriptions;
using Home.WebApi.UseCases.CalendarSubscriptions.GetCalendarSubscriptions;

namespace Home.Application.Tests.UseCases.CalendarSubscriptions.GetCalendarSubscriptions;

/// <summary>
/// What the Settings card reads. The feed address is a secret token in the URL for most providers,
/// so the household isolation here is not just tidiness.
/// </summary>
public class GetCalendarSubscriptionsInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetCalendarSubscriptionsPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static CalendarSubscription Subscription(
        long calendarSubscriptionID,
        Household household,
        string name,
        string url,
        DateTime? lastFetchedUTC = null,
        string? lastError = null)
        => new()
        {
            CalendarSubscriptionID = calendarSubscriptionID,
            Household = household,
            LastError = lastError,
            LastFetchedUTC = lastFetchedUTC,
            Name = name,
            Url = url
        };

    private Task HandleAsync()
        => new GetCalendarSubscriptionsInteractor().HandleAsync(
            new GetCalendarSubscriptionsInputPort(),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    private GetCalendarSubscriptionsApiResponse Response()
        => Ok<GetCalendarSubscriptionsApiResponse>(this.m_Presenter);

    [Fact]
    public async Task HandleAsync_ReturnsOurSubscriptionsByName()
    {
        _ = this.Database.Seed(
            Subscription(180, this.Ours, "Swim club", "https://swim.test/feed.ics"),
            Subscription(181, this.Ours, "Netball", "https://netball.test/feed.ics"));

        await this.HandleAsync();

        _ = this.Response().Subscriptions.Select(s => s.Name).Should().Equal(["Netball", "Swim club"]);
    }

    [Fact]
    public async Task HandleAsync_ShowsOnlyTheHostSoTheSecretInTheAddressIsNeverSentToTheBrowser()
    {
        _ = this.Database.Seed(Subscription(
            182, this.Ours, "Family", "https://calendar.google.test/ical/private-abc123/basic.ics"));

        await this.HandleAsync();

        _ = this.Response().Subscriptions.Single().Host.Should().Be("calendar.google.test");
    }

    [Fact]
    public async Task HandleAsync_CarriesWhenItLastReadAndWhyItCouldNot()
    {
        var _Fetched = new DateTime(2026, 9, 8, 6, 0, 0, DateTimeKind.Utc);

        _ = this.Database.Seed(
            Subscription(183, this.Ours, "Working", "https://swim.test/feed.ics", _Fetched),
            Subscription(184, this.Ours, "Broken", "https://gone.test/feed.ics", null, "The calendar could not be read."));

        await this.HandleAsync();

        var _Subscriptions = this.Response().Subscriptions;

        _ = _Subscriptions.Single(s => s.Name == "Working").LastFetchedUTC.Should().Be(_Fetched);
        _ = _Subscriptions.Single(s => s.Name == "Working").LastError.Should().BeNull();
        _ = _Subscriptions.Single(s => s.Name == "Broken").LastError.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HandleAsync_NeverReturnsAnotherHouseholdsSubscription()
    {
        _ = this.Database.Seed(
            Subscription(185, this.Ours, "Ours", "https://ours.test/feed.ics"),
            Subscription(985, this.Theirs, "Theirs", "https://theirs.test/private-token/feed.ics"));

        await this.HandleAsync();

        _ = this.Response().Subscriptions.Select(s => s.Name).Should().Equal(["Ours"]);
    }

    [Fact]
    public async Task HandleAsync_ReturnsNothingWhenTheHouseholdHasNoneRatherThanFailing()
    {
        await this.HandleAsync();

        _ = this.Response().Subscriptions.Should().BeEmpty();
    }

    #endregion Methods

}
