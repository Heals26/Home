using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.MealSlots.UpdateMealSlot;

public class UpdateMealSlotInputPortValidator : BaseValidator<UpdateMealSlotInputPort>
{

    #region Constructors

    public UpdateMealSlotInputPortValidator()
    {
        _ = this.RuleFor(r => r.Name.Value)
            .NotEmpty()
            .MaximumLength(50)
            .When(r => r.Name.HasBeenSet)
            .WithName("Name");

        _ = this.RuleFor(r => r.Sequence.Value)
            .GreaterThanOrEqualTo(0)
            .When(r => r.Sequence.HasBeenSet)
            .WithName("Sequence");

        _ = this.RuleFor(r => r.StartsAt.Value)
            .InclusiveBetween(TimeSpan.Zero, new TimeSpan(23, 59, 59))
            .When(r => r.StartsAt.HasBeenSet && r.StartsAt.Value != null)
            .WithName("StartsAt");
    }

    #endregion Constructors

}
