using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.MealPlanEntries.CreateMealPlanEntry;

public class CreateMealPlanEntryInputPortValidator : BaseValidator<CreateMealPlanEntryInputPort>
{

    #region Constructors

    public CreateMealPlanEntryInputPortValidator()
    {
        _ = this.RuleFor(r => r.Date)
            .NotEmpty();

        _ = this.RuleFor(r => r.RecipeID)
            .GreaterThan(0);
    }

    #endregion Constructors

}
