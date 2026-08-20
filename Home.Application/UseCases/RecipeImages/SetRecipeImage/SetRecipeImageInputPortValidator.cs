using FluentValidation;
using Home.Application.Infrastructure.Recipes;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.RecipeImages.SetRecipeImage;

public class SetRecipeImageInputPortValidator : BaseValidator<SetRecipeImageInputPort>
{

    #region Constructors

    public SetRecipeImageInputPortValidator()
    {
        _ = this.RuleFor(r => r.Content)
            .NotEmpty()
            .WithMessage("The upload contained no image.");

        _ = this.RuleFor(r => r.Content)
            .Must(c => c.Length <= RecipeImageLogic.MaximumContentBytes)
            .WithMessage("The photo is too large — keep it under 5 MB.");

        _ = this.RuleFor(r => r.Content)
            .Must(c => RecipeImageLogic.DetectContentType(c) != null)
            .WithMessage("That file isn't an image a browser can draw. Use a JPEG, PNG, WebP or GIF.")
            .When(r => r.Content.Length > 0);

        _ = this.RuleFor(r => r.RecipeID)
            .GreaterThan(0);
    }

    #endregion Constructors

}
