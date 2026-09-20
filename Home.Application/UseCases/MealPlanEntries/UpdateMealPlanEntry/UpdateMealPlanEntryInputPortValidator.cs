using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.MealPlanEntries.UpdateMealPlanEntry;

public class UpdateMealPlanEntryInputPortValidator : BaseValidator<UpdateMealPlanEntryInputPort>
{

    #region Constructors

    public UpdateMealPlanEntryInputPortValidator()
    {
        // An occasion is only ever its name, so an empty one would leave the day with a blank chip.
        _ = this.RuleFor(r => r.Title.Value)
            .NotEmpty()
            .MaximumLength(250)
            .When(r => r.Title.HasBeenSet)
            .WithName("Title");
    }

    #endregion Constructors

}
