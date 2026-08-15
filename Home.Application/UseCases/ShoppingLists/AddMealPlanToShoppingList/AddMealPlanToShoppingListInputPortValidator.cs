using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.ShoppingLists.AddMealPlanToShoppingList;

public class AddMealPlanToShoppingListInputPortValidator : BaseValidator<AddMealPlanToShoppingListInputPort>
{

    #region Constructors

    public AddMealPlanToShoppingListInputPortValidator()
    {
        _ = this.RuleFor(r => r.MealSlotID)
            .GreaterThan(0)
            .When(r => r.MealSlotID != null);

        _ = this.RuleFor(r => r.ShoppingListID)
            .GreaterThan(0);

        _ = this.RuleFor(r => r.ToDate)
            .GreaterThanOrEqualTo(r => r.FromDate);
    }

    #endregion Constructors

}
