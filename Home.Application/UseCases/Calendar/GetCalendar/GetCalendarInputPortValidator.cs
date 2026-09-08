using FluentValidation;
using Home.Application.Infrastructure.Calendar;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.Calendar.GetCalendar;

public class GetCalendarInputPortValidator : BaseValidator<GetCalendarInputPort>
{

    #region Constructors

    public GetCalendarInputPortValidator()
    {
        _ = this.RuleFor(r => r.TimeZoneID)
            .NotEmpty()
            .MaximumLength(100);

        _ = this.RuleFor(r => r.ToDate)
            .GreaterThanOrEqualTo(r => r.FromDate)
            .Must((r, toDate) => toDate.DayNumber - r.FromDate.DayNumber < CalendarValues.MaximumWindowDays)
            .WithMessage($"Ask for at most {CalendarValues.MaximumWindowDays} days at a time.");
    }

    #endregion Constructors

}
