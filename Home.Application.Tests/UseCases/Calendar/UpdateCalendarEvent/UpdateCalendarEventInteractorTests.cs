using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Calendar.UpdateCalendarEvent;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.Calendar.UpdateCalendarEvent;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Calendar.UpdateCalendarEvent;

public class UpdateCalendarEventInteractorTests : InteractorTest
{

    #region Fields

    private readonly UpdateCalendarEventPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static UpdateCalendarEventInputPort Input(long calendarEventID, string title, params long[] memberUserIDs)
        => new(
            calendarEventID,
            0,
            new DateOnly(2026, 9, 16),
            null,
            CalendarRecurrenceFrequency.None,
            1,
            true,
            null,
            memberUserIDs,
            "Bring goggles",
            false,
            null,
            new DateOnly(2026, 9, 16),
            null,
            null,
            title);

    private Task HandleAsync(UpdateCalendarEventInputPort input)
        => new UpdateCalendarEventInteractor().HandleAsync(input, this.m_Presenter, this.Services().Build(), CancellationToken.None);

    [Fact]
    public async Task HandleAsync_ReplacesTheEventAndReconcilesWhoIsOnIt()
    {
        var _Second = new User() { UserID = 101, Email = "second@ours.test", FirstName = "Bea", Household = this.Ours, LastName = "Member" };
        var _Event = CalendarTestData.TimedEvent(150, this.Ours, "Swimming", new DateOnly(2026, 9, 15), new TimeOnly(9, 0), new TimeOnly(10, 0));
        _Event.Members = [new CalendarEventMember() { User = this.Member }];

        _ = this.Database.Seed(_Event, _Second);

        await this.HandleAsync(Input(150, "Swimming carnival", _Second.UserID));

        var _Stored = this.Stored<CalendarEvent>().Single();

        _ = _Stored.Title.Should().Be("Swimming carnival");
        _ = _Stored.IsAllDay.Should().BeTrue();
        _ = _Stored.StartTime.Should().BeNull("the update made it all day");
        _ = _Stored.Notes.Should().Be("Bring goggles");
        _ = this.Stored<CalendarEventMember>().Select(m => m.UserID).Should().Equal(
            [_Second.UserID],
            "Ada came off and Bea went on");
        _ = this.m_Presenter.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task HandleAsync_PresentsNotFoundForAnotherHouseholdsEvent()
    {
        _ = this.Database.Seed(CalendarTestData.AllDayEvent(950, this.Theirs, "Their event", new DateOnly(2026, 9, 15)));

        await this.HandleAsync(Input(950, "Hijacked"));

        ShouldBeNotFound(this.m_Presenter);
        _ = this.Stored<CalendarEvent>().Single().Title.Should().Be("Their event");
    }

    [Fact]
    public async Task HandleAsync_RefusesToChangeASubscribedEvent()
    {
        var _Event = CalendarTestData.AllDayEvent(150, this.Ours, "School photos", new DateOnly(2026, 9, 15));
        _Event.Subscription = new CalendarSubscription() { CalendarSubscriptionID = 180, Household = this.Ours, Name = "School", Url = "https://school.test/feed.ics" };

        _ = this.Database.Seed(_Event);

        await this.HandleAsync(Input(150, "Renamed"));

        _ = this.m_Presenter.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        _ = this.Stored<CalendarEvent>().Single().Title.Should().Be("School photos");
    }

    #endregion Methods

}
