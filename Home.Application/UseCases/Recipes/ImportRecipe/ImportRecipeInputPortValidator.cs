using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.Recipes.ImportRecipe;

public class ImportRecipeInputPortValidator : BaseValidator<ImportRecipeInputPort>
{

    #region Constructors

    public ImportRecipeInputPortValidator()
    {
        _ = this.RuleFor(r => r.Url)
            .NotEmpty()
            .MaximumLength(2000)
            .Must(BeAnAbsoluteWebAddress)
            .WithMessage("The URL must be a full http or https web address.");
    }

    #endregion Constructors

    #region Methods

    private static bool BeAnAbsoluteWebAddress(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var _Uri)
            && (_Uri.Scheme == Uri.UriSchemeHttp || _Uri.Scheme == Uri.UriSchemeHttps);

    #endregion Methods

}
