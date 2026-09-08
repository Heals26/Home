using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Calendar.GetCalendar;
using Home.Application.UseCases.Calendar.Models;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.Calendar.GetCalendar;
using Home.WebApi.UseCases.Calendar.GetCalendar;
using Home.WebApi.UseCases.Calendar.Models;

namespace Home.Application.Tests.UseCases.Calendar.GetCalendar;

/// <summary>
/// The projection across events, meals and tasks that the calendar and the dashboard both read.
/// Tested against a real database through the real presenter, because a projection that forgets
/// a navigation is the bug class this repo keeps meeting.
/// </summary>
public class GetCalendarInteractorTests : InteractorTest
{

    #region Fields

    private readonly GetCalendarPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static DateOnly D(int year, int month, int day)
        => new(year, month, day);

    private Task HandleAsync(DateOnly fromDate, DateOnly toDate, string timeZoneID = "Australia/Brisbane")
        => new GetCalendarInteractor().HandleAsync(
            new GetCalendarInputPort(fromDate, timeZoneID, toDate),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    private List<CalendarItemDto> ItemsOn(DateOnly date)
        => Ok<GetCalendarApiResponse>(this.m_Presenter).Days.Single(d => d.Date == date).Items;

    [Fact]
    public async Task HandleAsync_FoldsEventsMealsAndTasksIntoOneDayInTimeOrder()
    {
        var _Dinner = new MealSlot() { Household = this.Ours, MealSlotID = 110, Name = "Dinner", Sequence = 3, StartsAt = new TimeSpan(18, 30, 0) };
        var _Day = D(2026, 9, 15);

        _ = this.Database.Seed(
            CalendarTestData.AllDayEvent(150, this.Ours, "Bin day", _Day),
            CalendarTestData.TimedEvent(151, this.Ours, "Dentist", _Day, new TimeOnly(9, 0), new TimeOnly(10, 0)),
            CalendarTestData.Meal(160, this.Ours, "Bolognese", _Day, _Dinner),
            CalendarTestData.Task(170, this.Ours, "Homework", _Day, new TimeSpan(16, 0, 0), this.Member));

        await this.HandleAsync(_Day, _Day);

        var _Items = this.ItemsOn(_Day);

        _ = _Items.Select(i => i.Title).Should().Equal(
            ["Bin day", "Dentist", "Homework", "Bolognese"],
            "all-day things come first, then everything with a time in time order");
        _ = _Items.Select(i => i.Kind).Should().Equal(
            CalendarItemKind.Event, CalendarItemKind.Event, CalendarItemKind.Task, CalendarItemKind.Meal);
        _ = _Items.Select(i => i.StartsAt).Should().Equal(
            null, new TimeOnly(9, 0), new TimeOnly(16, 0), new TimeOnly(18, 30));
        _ = _Items.Single(i => i.Kind == CalendarItemKind.Meal).Subtitle.Should().Be("Dinner", "the presenter reads the slot, so the query has to load it");
        _ = _Items.Single(i => i.Kind == CalendarItemKind.Task).Subtitle.Should().Be("Ada Member", "the presenter reads the assignee, so the query has to load it");
    }

    [Fact]
    public async Task HandleAsync_ReturnsEveryDayAskedForEvenWhenEmpty()
    {
        await this.HandleAsync(D(2026, 9, 14), D(2026, 9, 20));

        var _Days = Ok<GetCalendarApiResponse>(this.m_Presenter).Days;

        _ = _Days.Select(d => d.Date).Should().Equal(Enumerable.Range(0, 7).Select(i => D(2026, 9, 14).AddDays(i)));
        _ = _Days.Should().OnlyContain(d => d.Items.Count == 0);
    }

    [Fact]
    public async Task HandleAsync_PlacesATimedEventOnTheViewersLocalDay()
    {
        // 8am in Brisbane is 22:00 UTC the evening before, which is 3pm the day before in Los Angeles.
        _ = this.Database.Seed(
            CalendarTestData.TimedEvent(150, this.Ours, "Swimming", D(2026, 9, 15), new TimeOnly(8, 0), new TimeOnly(9, 0)));

        await this.HandleAsync(D(2026, 9, 13), D(2026, 9, 16), timeZoneID: "America/Los_Angeles");

        var _Response = Ok<GetCalendarApiResponse>(this.m_Presenter);

        _ = _Response.Days.Single(d => d.Date == D(2026, 9, 15)).Items.Should().BeEmpty();

        var _Item = _Response.Days.Single(d => d.Date == D(2026, 9, 14)).Items.Single();

        _ = _Item.StartsAt.Should().Be(new TimeOnly(15, 0));
        _ = _Item.StartUTC.Should().Be(new DateTime(2026, 9, 14, 22, 0, 0));
    }

    [Fact]
    public async Task HandleAsync_ExpandsASeriesAndLeavesOutASkippedOccurrence()
    {
        var _Series = CalendarTestData.TimedEvent(150, this.Ours, "Piano", D(2026, 9, 7), new TimeOnly(16, 0), new TimeOnly(16, 30));
        _Series.Frequency = CalendarRecurrenceFrequency.Weekly;
        _Series.DaysOfWeek = (1 << 1) | (1 << 3);
        _Series.Exceptions = [new CalendarEventException() { OccurrenceDate = D(2026, 9, 9) }];
        _Series.Members = [new CalendarEventMember() { User = this.Member }];

        _ = this.Database.Seed(_Series);

        await this.HandleAsync(D(2026, 9, 7), D(2026, 9, 16));

        var _Items = Ok<GetCalendarApiResponse>(this.m_Presenter).Days.SelectMany(d => d.Items).ToList();

        _ = _Items.Select(i => i.Date).Should().Equal(
            [D(2026, 9, 7), D(2026, 9, 14), D(2026, 9, 16)],
            "Wednesday the 9th was skipped");
        _ = _Items.Should().OnlyContain(i => i.IsRecurring && i.ID == 150);
        _ = _Items.Select(i => i.OccurrenceDate).Should().Equal(D(2026, 9, 7), D(2026, 9, 14), D(2026, 9, 16));
        _ = _Items[0].People.Should().Equal(["Ada Member"], "the presenter names the people, so the query has to load them");
    }

    [Fact]
    public async Task HandleAsync_SpreadsAMultiDayEventAcrossEachOfItsDays()
    {
        _ = this.Database.Seed(CalendarTestData.AllDayEvent(150, this.Ours, "Camping", D(2026, 9, 18), lengthInDays: 2));

        await this.HandleAsync(D(2026, 9, 19), D(2026, 9, 25));

        var _Items = Ok<GetCalendarApiResponse>(this.m_Presenter).Days.SelectMany(d => d.Items).ToList();

        _ = _Items.Select(i => i.Date).Should().Equal([D(2026, 9, 19), D(2026, 9, 20)], "the 18th is outside the window");
        _ = _Items.Should().OnlyContain(i => i.StartDate == D(2026, 9, 18) && i.EndDate == D(2026, 9, 20));
    }

    [Fact]
    public async Task HandleAsync_MarksASubscribedEventReadOnlyAndNamesItsCalendar()
    {
        var _Event = CalendarTestData.AllDayEvent(150, this.Ours, "School photos", D(2026, 9, 15));
        _Event.Subscription = new CalendarSubscription() { CalendarSubscriptionID = 180, Household = this.Ours, Name = "School", Url = "https://school.test/feed.ics" };

        _ = this.Database.Seed(_Event);

        await this.HandleAsync(D(2026, 9, 15), D(2026, 9, 15));

        var _Item = this.ItemsOn(D(2026, 9, 15)).Single();

        _ = _Item.Kind.Should().Be(CalendarItemKind.Subscribed);
        _ = _Item.IsReadOnly.Should().BeTrue();
        _ = _Item.Subtitle.Should().Be("School", "the presenter reads the subscription, so the query has to load it");
    }

    [Fact]
    public async Task HandleAsync_NeverShowsAnotherHouseholdsDay()
    {
        var _Day = D(2026, 9, 15);

        _ = this.Database.Seed(
            CalendarTestData.AllDayEvent(150, this.Ours, "Ours", _Day),
            CalendarTestData.AllDayEvent(950, this.Theirs, "Their event", _Day),
            CalendarTestData.Meal(960, this.Theirs, "Their dinner", _Day),
            CalendarTestData.Task(970, this.Theirs, "Their chore", _Day));

        await this.HandleAsync(_Day, _Day);

        _ = this.ItemsOn(_Day).Select(i => i.Title).Should().Equal(["Ours"]);
    }

    #endregion Methods

}
