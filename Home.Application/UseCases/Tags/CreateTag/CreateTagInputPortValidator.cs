using FluentValidation;
using Home.Application.Infrastructure.Validation;
using Home.Application.UseCases.Tags.Models;

namespace Home.Application.UseCases.Tags.CreateTag;

public class CreateTagInputPortValidator : BaseValidator<CreateTagInputPort>
{

    #region Constructors

    public CreateTagInputPortValidator()
    {
        _ = this.RuleFor(r => r.Colour)
            .Matches(TagValues.ColourPattern)
            .WithMessage("'Colour' must be a hex colour in the form #RRGGBB.");

        _ = this.RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(50);
    }

    #endregion Constructors

}
