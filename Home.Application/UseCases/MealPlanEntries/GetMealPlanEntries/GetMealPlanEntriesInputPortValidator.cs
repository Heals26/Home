using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.MealPlanEntries.GetMealPlanEntries;

public class GetMealPlanEntriesInputPortValidator : BaseValidator<GetMealPlanEntriesInputPort>
{

    #region Constructors

    public GetMealPlanEntriesInputPortValidator()
    {
        _ = this.RuleFor(r => r.ToDate)
            .GreaterThanOrEqualTo(r => r.FromDate);
    }

    #endregion Constructors

}
