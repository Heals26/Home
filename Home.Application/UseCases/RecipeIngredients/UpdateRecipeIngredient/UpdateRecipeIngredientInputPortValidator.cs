using FluentValidation;
using Home.Application.Infrastructure.Recipes;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.RecipeIngredients.UpdateRecipeIngredient;

public class UpdateRecipeIngredientInputPortValidator : BaseValidator<UpdateRecipeIngredientInputPort>
{

    #region Constructors

    public UpdateRecipeIngredientInputPortValidator()
    {
        _ = this.RuleFor(r => r.Amount.Value)
            .GreaterThan(0)
            .When(r => r.Amount.HasBeenSet && r.Amount.Value != null)
            .WithName("Amount");

        _ = this.RuleFor(r => r.Name.Value)
            .NotEmpty()
            .MaximumLength(200)
            .When(r => r.Name.HasBeenSet)
            .WithName("Name");

        _ = this.RuleFor(r => r.Unit.Value)
            .Must(MeasurementUnitLogic.IsDefined)
            .When(r => r.Unit.HasBeenSet)
            .WithMessage("Unit is not one of the known measurements.")
            .WithName("Unit");
    }

    #endregion Constructors

}
