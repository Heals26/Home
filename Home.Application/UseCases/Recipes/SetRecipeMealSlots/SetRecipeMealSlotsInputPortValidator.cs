using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.Recipes.SetRecipeMealSlots;

public class SetRecipeMealSlotsInputPortValidator : BaseValidator<SetRecipeMealSlotsInputPort>
{

    #region Constructors

    public SetRecipeMealSlotsInputPortValidator()
    {
        // An empty list is how a recipe is taken out of every meal, so only null is refused.
        _ = this.RuleForEach(r => r.MealSlotIDs)
            .GreaterThan(0);

        _ = this.RuleFor(r => r.MealSlotIDs)
            .NotNull();

        _ = this.RuleFor(r => r.RecipeID)
            .GreaterThan(0);
    }

    #endregion Constructors

}
