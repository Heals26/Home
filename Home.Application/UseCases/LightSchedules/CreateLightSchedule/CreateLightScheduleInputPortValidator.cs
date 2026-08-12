using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.LightSchedules.CreateLightSchedule;

public class CreateLightScheduleInputPortValidator : BaseValidator<CreateLightScheduleInputPort>
{

    #region Constructors

    public CreateLightScheduleInputPortValidator()
    {
        _ = this.RuleFor(r => r.Name).NotEmpty().MaximumLength(250);

        // Seven bits, and at least one of them set — zero would be a schedule that never fires.
        _ = this.RuleFor(r => r.DaysOfWeek).InclusiveBetween(1, 127);

        _ = this.RuleFor(r => r.TimeOfDay)
            .Must(t => t >= TimeSpan.Zero && t < TimeSpan.FromDays(1))
            .WithMessage("Time of day must fall within a single day.");
    }

    #endregion Constructors

}
