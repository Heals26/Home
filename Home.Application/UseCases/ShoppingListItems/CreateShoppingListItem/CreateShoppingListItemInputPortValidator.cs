using FluentValidation;
using Home.Application.Infrastructure.Recipes;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.ShoppingListItems.CreateShoppingListItem;

public class CreateShoppingListItemInputPortValidator : BaseValidator<CreateShoppingListItemInputPort>
{

    #region Constructors

    public CreateShoppingListItemInputPortValidator()
    {
        _ = this.RuleFor(r => r.Amount)
            .GreaterThan(0)
            .When(r => r.Amount != null);

        _ = this.RuleFor(r => r.Cost)
            .GreaterThanOrEqualTo(0)
            .When(r => r.Cost != null);

        _ = this.RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(200);

        _ = this.RuleFor(r => r.ShoppingListID)
            .GreaterThan(0);

        _ = this.RuleFor(r => r.Unit)
            .Must(MeasurementUnitLogic.IsDefined)
            .WithMessage("Unit is not one of the known measurements.");
    }

    #endregion Constructors

}
