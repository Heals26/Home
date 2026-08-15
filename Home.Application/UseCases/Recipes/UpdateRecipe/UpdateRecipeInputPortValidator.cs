using FluentValidation;
using Home.Application.Infrastructure.Recipes;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.Recipes.UpdateRecipe;

public class UpdateRecipeInputPortValidator : BaseValidator<UpdateRecipeInputPort>
{

    #region Constructors

    public UpdateRecipeInputPortValidator()
    {
        _ = this.RuleFor(r => r.Complexity.Value)
            .Must(RecipeComplexityLogic.IsDefined)
            .When(r => r.Complexity.HasBeenSet)
            .WithMessage("Complexity is not one of the known values.")
            .WithName("Complexity");

        _ = this.RuleFor(r => r.CookMinutes.Value)
            .InclusiveBetween(0, RecipeValues.MaximumMinutes)
            .When(r => r.CookMinutes.HasBeenSet && r.CookMinutes.Value != null)
            .WithName("CookMinutes");

        // An empty address is a deliberate clearing, so only a value that is there is checked.
        _ = this.RuleFor(r => r.ImageUrl.Value)
            .MaximumLength(2048)
            .Must(RecipeImageLogic.IsAWebAddress)
            .When(r => r.ImageUrl.HasBeenSet && !string.IsNullOrWhiteSpace(r.ImageUrl.Value))
            .WithMessage("The image address must be a full http or https web address.")
            .WithName("ImageUrl");

        _ = this.RuleFor(r => r.Name.Value)
            .NotEmpty()
            .MaximumLength(250)
            .When(r => r.Name.HasBeenSet)
            .WithName("Name");

        _ = this.RuleFor(r => r.PrepMinutes.Value)
            .InclusiveBetween(0, RecipeValues.MaximumMinutes)
            .When(r => r.PrepMinutes.HasBeenSet && r.PrepMinutes.Value != null)
            .WithName("PrepMinutes");

        _ = this.RuleFor(r => r.Servings.Value)
            .InclusiveBetween(1, RecipeValues.MaximumServings)
            .When(r => r.Servings.HasBeenSet && r.Servings.Value != null)
            .WithName("Servings");
    }

    #endregion Constructors

}
