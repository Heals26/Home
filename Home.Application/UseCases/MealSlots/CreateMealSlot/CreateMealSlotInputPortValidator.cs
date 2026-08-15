using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.MealSlots.CreateMealSlot;

public class CreateMealSlotInputPortValidator : BaseValidator<CreateMealSlotInputPort>
{

    #region Constructors

    public CreateMealSlotInputPortValidator()
    {
        _ = this.RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(50);

        _ = this.RuleFor(r => r.StartsAt)
            .InclusiveBetween(TimeSpan.Zero, new TimeSpan(23, 59, 59))
            .When(r => r.StartsAt != null);
    }

    #endregion Constructors

}
