using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Calendar.GetCalendarEvent;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.Calendar.GetCalendarEvent;
using Home.WebApi.UseCases.Calendar.GetCalendarEvent;

namespace Home.Application.Tests.UseCases.Calendar.GetCalendarEvent;

/// <summary>
/// The read behind the editor. Driven through the real presenter, which reads the members and the
/// subscription off the event, so a query that forgets either fails here rather than on screen.
/// </summary>
public class GetCalendarEventInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetCalendarEventPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Task HandleAsync(long calendarEventID)
        => new GetCalendarEventInteractor().HandleAsync(
            new GetCalendarEventInputPort(calendarEventID),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    private GetCalendarEventApiResponse Response()
        => Ok<GetCalendarEventApiResponse>(this.m_Presenter);

    [Fact]
    public async Task HandleAsync_ReturnsTheEventWithEverythingTheEditorNeeds()
    {
        _ = this.Database.Seed(CalendarTestData.TimedEvent(
            150, this.Ours, "Swimming", new DateOnly(2026, 9, 15), new TimeOnly(16, 0), new TimeOnly(17, 0)));

        await this.HandleAsync(150);

        var _Response = this.Response();

        _ = _Response.CalendarEventID.Should().Be(150);
        _ = _Response.Title.Should().Be("Swimming");
        _ = _Response.StartTime.Should().Be(new TimeOnly(16, 0));
        _ = _Response.TimeZoneID.Should().Be("Australia/Brisbane");
        _ = _Response.IsReadOnly.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ProjectsTheMembersTheEventIsFor()
    {
        var _Event = CalendarTestData.AllDayEvent(151, this.Ours, "Sports day", new DateOnly(2026, 9, 16));

        _Event.Members = [new CalendarEventMember() { CalendarEvent = _Event, User = this.Member }];

        _ = this.Database.Seed(_Event);

        await this.HandleAsync(151);

        _ = this.Response().MemberUserIDs.Should().Equal([this.Member.UserID], "the presenter reads Members, so the query has to load it");
    }

    [Fact]
    public async Task HandleAsync_MarksAFeedsEventReadOnlyAndNamesTheCalendarItCameFrom()
    {
        var _Subscription = new CalendarSubscription()
        {
            CalendarSubscriptionID = 180,
            Household = this.Ours,
            Name = "Springfield Primary",
            Url = "https://school.test/feed.ics"
        };

        var _Event = CalendarTestData.AllDayEvent(152, this.Ours, "School photos", new DateOnly(2026, 9, 17));

        _Event.Subscription = _Subscription;

        _ = this.Database.Seed(_Subscription, _Event);

        await this.HandleAsync(152);

        var _Response = this.Response();

        _ = _Response.IsReadOnly.Should().BeTrue();
        _ = _Response.SubscriptionName.Should().Be("Springfield Primary", "the presenter reads Subscription, so the query has to load it");
    }

    [Fact]
    public async Task HandleAsync_KeepsTheRepeatSoTheEditorCanShowIt()
    {
        var _Event = CalendarTestData.TimedEvent(
            153, this.Ours, "Swimming", new DateOnly(2026, 9, 15), new TimeOnly(16, 0), new TimeOnly(17, 0));

        _Event.Frequency = CalendarRecurrenceFrequency.Weekly;
        _Event.DaysOfWeek = 1 << (int)DayOfWeek.Tuesday;
        _Event.Interval = 2;
        _Event.RepeatUntil = new DateOnly(2026, 12, 15);

        _ = this.Database.Seed(_Event);

        await this.HandleAsync(153);

        var _Response = this.Response();

        _ = _Response.Frequency.Should().Be(CalendarRecurrenceFrequency.Weekly);
        _ = _Response.DaysOfWeek.Should().Be(1 << (int)DayOfWeek.Tuesday);
        _ = _Response.Interval.Should().Be(2);
        _ = _Response.RepeatUntil.Should().Be(new DateOnly(2026, 12, 15));
    }

    [Fact]
    public async Task HandleAsync_PresentsNotFoundForAnotherHouseholdsEvent()
    {
        _ = this.Database.Seed(CalendarTestData.AllDayEvent(950, this.Theirs, "Their bin day", new DateOnly(2026, 9, 15)));

        await this.HandleAsync(950);

        ShouldBeNotFound(this.m_Presenter);
    }

    [Fact]
    public async Task HandleAsync_PresentsNotFoundForAnEventThatDoesNotExist()
    {
        await this.HandleAsync(404);

        ShouldBeNotFound(this.m_Presenter);
    }

    #endregion Methods

}
