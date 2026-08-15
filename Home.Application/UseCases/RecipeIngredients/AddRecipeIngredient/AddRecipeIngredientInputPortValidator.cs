using FluentValidation;
using Home.Application.Infrastructure.Recipes;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.RecipeIngredients.AddRecipeIngredient;

public class AddRecipeIngredientInputPortValidator : BaseValidator<AddRecipeIngredientInputPort>
{

    #region Constructors

    public AddRecipeIngredientInputPortValidator()
    {
        _ = this.RuleFor(r => r.Amount)
            .GreaterThan(0)
            .When(r => r.Amount != null);

        _ = this.RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(200);

        _ = this.RuleFor(r => r.RecipeID)
            .GreaterThan(0);

        _ = this.RuleFor(r => r.Unit)
            .Must(MeasurementUnitLogic.IsDefined)
            .WithMessage("Unit is not one of the known measurements.");
    }

    #endregion Constructors

}
