using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Calendar.DeleteCalendarEvent;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.Calendar.DeleteCalendarEvent;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Calendar.DeleteCalendarEvent;

public class DeleteCalendarEventInteractorTests : InteractorTest
{

    #region Fields

    private readonly DeleteCalendarEventPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private Task HandleAsync(long calendarEventID, DateOnly? occurrenceDate = null)
        => new DeleteCalendarEventInteractor().HandleAsync(
            new DeleteCalendarEventInputPort(calendarEventID, occurrenceDate),
            this.m_Presenter,
            this.Services().Build(),
            CancellationToken.None);

    private CalendarEvent SeedWeeklySeries()
    {
        var _Series = CalendarTestData.TimedEvent(150, this.Ours, "Piano", new DateOnly(2026, 9, 7), new TimeOnly(16, 0), new TimeOnly(16, 30));
        _Series.Frequency = CalendarRecurrenceFrequency.Weekly;

        _ = this.Database.Seed(_Series);

        return _Series;
    }

    [Fact]
    public async Task HandleAsync_RemovesAOneOffOutright()
    {
        _ = this.Database.Seed(CalendarTestData.AllDayEvent(150, this.Ours, "Bin day", new DateOnly(2026, 9, 15)));

        await this.HandleAsync(150);

        _ = this.Stored<CalendarEvent>().Should().BeEmpty();
        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task HandleAsync_SkipsOneOccurrenceOfASeriesAndLeavesTheSeriesStanding()
    {
        _ = this.SeedWeeklySeries();

        await this.HandleAsync(150, new DateOnly(2026, 9, 14));

        _ = this.Stored<CalendarEvent>().Should().ContainSingle();
        _ = this.Stored<CalendarEventException>().Single().OccurrenceDate.Should().Be(new DateOnly(2026, 9, 14));
    }

    [Fact]
    public async Task HandleAsync_SkippingTheSameOccurrenceTwiceIsOneSkip()
    {
        _ = this.SeedWeeklySeries();

        await this.HandleAsync(150, new DateOnly(2026, 9, 14));
        await this.HandleAsync(150, new DateOnly(2026, 9, 14));

        _ = this.Stored<CalendarEventException>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_AnOccurrenceDateOnAOneOffRemovesTheWholeThing()
    {
        _ = this.Database.Seed(CalendarTestData.AllDayEvent(150, this.Ours, "Bin day", new DateOnly(2026, 9, 15)));

        await this.HandleAsync(150, new DateOnly(2026, 9, 15));

        _ = this.Stored<CalendarEvent>().Should().BeEmpty("a one-off has no series to leave standing");
    }

    [Fact]
    public async Task HandleAsync_PresentsNotFoundForAnotherHouseholdsEvent()
    {
        _ = this.Database.Seed(CalendarTestData.AllDayEvent(950, this.Theirs, "Their event", new DateOnly(2026, 9, 15)));

        await this.HandleAsync(950);

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<CalendarEvent>().Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_RefusesToRemoveASubscribedEvent()
    {
        var _Event = CalendarTestData.AllDayEvent(150, this.Ours, "School photos", new DateOnly(2026, 9, 15));
        _Event.Subscription = new CalendarSubscription() { CalendarSubscriptionID = 180, Household = this.Ours, Name = "School", Url = "https://school.test/feed.ics" };

        _ = this.Database.Seed(_Event);

        await this.HandleAsync(150);

        _ = this.m_Presenter.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        _ = this.Stored<CalendarEvent>().Should().ContainSingle("it would only come back on the next refresh");
    }

    #endregion Methods

}
