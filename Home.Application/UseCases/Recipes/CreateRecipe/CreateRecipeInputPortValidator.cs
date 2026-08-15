using FluentValidation;
using Home.Application.Infrastructure.Recipes;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.Recipes.CreateRecipe;

public class CreateRecipeInputPortValidator : BaseValidator<CreateRecipeInputPort>
{

    #region Constructors

    public CreateRecipeInputPortValidator()
    {
        _ = this.RuleFor(r => r.Complexity)
            .Must(RecipeComplexityLogic.IsDefined)
            .WithMessage("Complexity is not one of the known values.");

        _ = this.RuleFor(r => r.CookMinutes)
            .InclusiveBetween(0, RecipeValues.MaximumMinutes)
            .When(r => r.CookMinutes != null);

        _ = this.RuleFor(r => r.ImageUrl)
            .MaximumLength(2048)
            .Must(RecipeImageLogic.IsAWebAddress)
            .When(r => !string.IsNullOrWhiteSpace(r.ImageUrl))
            .WithMessage("The image address must be a full http or https web address.");

        _ = this.RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(250);

        _ = this.RuleFor(r => r.PrepMinutes)
            .InclusiveBetween(0, RecipeValues.MaximumMinutes)
            .When(r => r.PrepMinutes != null);

        _ = this.RuleFor(r => r.Servings)
            .InclusiveBetween(1, RecipeValues.MaximumServings)
            .When(r => r.Servings != null);
    }

    #endregion Constructors

}
