using FluentAssertions;
using Home.Application.Tests.Infrastructure;
using Home.Application.UseCases.Calendar.CreateCalendarEvent;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.WebApi.Presenters.Calendar.CreateCalendarEvent;
using Microsoft.AspNetCore.Mvc;

namespace Home.Application.Tests.UseCases.Calendar.CreateCalendarEvent;

public class CreateCalendarEventInteractorTests : InteractorTest
{

    #region Fields

    private readonly CreateCalendarEventPresenter m_Presenter = new(Mapper);

    #endregion Fields

    #region Methods

    private static CreateCalendarEventInputPort Input(
        bool isAllDay = false,
        CalendarRecurrenceFrequency frequency = CalendarRecurrenceFrequency.None,
        int daysOfWeek = 0,
        params long[] memberUserIDs)
        => new(
            daysOfWeek,
            new DateOnly(2026, 9, 15),
            new TimeOnly(10, 0),
            frequency,
            1,
            isAllDay,
            "  The pool ",
            memberUserIDs,
            null,
            false,
            null,
            new DateOnly(2026, 9, 15),
            new TimeOnly(9, 0),
            "Australia/Brisbane",
            "  Swimming ");

    private Task HandleAsync(CreateCalendarEventInputPort input)
        => new CreateCalendarEventInteractor().HandleAsync(input, this.m_Presenter, this.Services().Build(), CancellationToken.None);

    [Fact]
    public async Task HandleAsync_StoresTheEventAgainstTheHouseholdAndTrimsWhatWasTyped()
    {
        await this.HandleAsync(Input());

        var _Stored = this.Stored<CalendarEvent>().Single(e => e.Household.HouseholdID == OurHouseholdID);

        _ = _Stored.Title.Should().Be("Swimming");
        _ = _Stored.Location.Should().Be("The pool");
        _ = _Stored.StartTime.Should().Be(new TimeOnly(9, 0));
        _ = _Stored.TimeZoneID.Should().Be("Australia/Brisbane");
        _ = this.m_Presenter.Result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task HandleAsync_PutsOnlyOurOwnMembersOnTheEvent()
    {
        _ = this.Database.Seed(this.Member, this.Neighbour);

        await this.HandleAsync(Input(memberUserIDs: [this.Member.UserID, this.Neighbour.UserID]));

        _ = this.Stored<CalendarEventMember>().Select(m => m.UserID).Should().Equal(
            [this.Member.UserID],
            "a member ID from another household is left off rather than putting a stranger on the event");
    }

    [Fact]
    public async Task HandleAsync_AnAllDayEventCarriesNoTimesAndNoZone()
    {
        await this.HandleAsync(Input(isAllDay: true));

        var _Stored = this.Stored<CalendarEvent>().Single();

        _ = _Stored.IsAllDay.Should().BeTrue();
        _ = _Stored.StartTime.Should().BeNull();
        _ = _Stored.EndTime.Should().BeNull();
        _ = _Stored.TimeZoneID.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_AWeeklyEventWithNoDayChosenRepeatsOnItsStartDay()
    {
        // 15 September 2026 is a Tuesday, bit 2.
        await this.HandleAsync(Input(frequency: CalendarRecurrenceFrequency.Weekly));

        _ = this.Stored<CalendarEvent>().Single().DaysOfWeek.Should().Be(1 << 2);
    }

    [Fact]
    public async Task HandleAsync_AOneOffCarriesNoRepeatRule()
    {
        await this.HandleAsync(Input(daysOfWeek: 0x7F));

        var _Stored = this.Stored<CalendarEvent>().Single();

        _ = _Stored.DaysOfWeek.Should().Be(0, "days of the week mean nothing on a one-off");
        _ = _Stored.Interval.Should().Be(1);
    }

    #endregion Methods

}
