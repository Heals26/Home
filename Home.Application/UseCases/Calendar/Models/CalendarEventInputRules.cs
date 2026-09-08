using FluentValidation;
using Home.Domain.Enumerations;

namespace Home.Application.UseCases.Calendar.Models;

/// <summary>
/// The validation both event writes share. Kept in one place so create and update always agree
/// on what an event is allowed to be.
/// </summary>
public static class CalendarEventInputRules
{

    #region Methods

    public static void AddEventRules<TInputPort>(this AbstractValidator<TInputPort> validator)
        where TInputPort : ICalendarEventInput
    {
        _ = validator.RuleFor(r => r.EndDate)
            .GreaterThanOrEqualTo(r => r.StartDate)
            .WithMessage("An event cannot end before it starts.");

        _ = validator.RuleFor(r => r.EndTime)
            .NotNull()
            .When(r => !r.IsAllDay)
            .WithMessage("Give the event an end time, or make it all day.");

        _ = validator.RuleFor(r => r.EndTime)
            .GreaterThan(r => r.StartTime)
            .When(r => !r.IsAllDay && r.StartTime != null && r.EndTime != null && r.StartDate == r.EndDate)
            .WithMessage("An event cannot end before it starts.");

        _ = validator.RuleFor(r => r.Interval)
            .InclusiveBetween(1, 99);

        _ = validator.RuleFor(r => r.Location)
            .MaximumLength(250);

        _ = validator.RuleFor(r => r.Notes)
            .MaximumLength(2000);

        _ = validator.RuleFor(r => r.RepeatUntil)
            .GreaterThanOrEqualTo(r => r.StartDate)
            .When(r => r.RepeatUntil != null && r.Frequency != CalendarRecurrenceFrequency.None)
            .WithMessage("A repeat cannot end before the event starts.");

        _ = validator.RuleFor(r => r.StartTime)
            .NotNull()
            .When(r => !r.IsAllDay)
            .WithMessage("Give the event a start time, or make it all day.");

        _ = validator.RuleFor(r => r.TimeZoneID)
            .NotEmpty()
            .MaximumLength(100)
            .Must(id => id != null && TimeZoneInfo.TryFindSystemTimeZoneById(id, out _))
            .When(r => !r.IsAllDay)
            .WithMessage("The time zone was not recognised.");

        _ = validator.RuleFor(r => r.Title)
            .NotEmpty()
            .MaximumLength(250);
    }

    #endregion Methods

}
