using FluentValidation;
using Home.Application.Infrastructure.Validation;
using Home.Application.UseCases.Tags.Models;

namespace Home.Application.UseCases.Tags.UpdateTag;

public class UpdateTagInputPortValidator : BaseValidator<UpdateTagInputPort>
{

    #region Constructors

    public UpdateTagInputPortValidator()
    {
        _ = this.RuleFor(r => r.Colour.Value)
            .Matches(TagValues.ColourPattern)
            .When(r => r.Colour.HasBeenSet)
            .WithMessage("'Colour' must be a hex colour in the form #RRGGBB.")
            .WithName("Colour");

        _ = this.RuleFor(r => r.Name.Value)
            .NotEmpty()
            .MaximumLength(50)
            .When(r => r.Name.HasBeenSet)
            .WithName("Name");
    }

    #endregion Constructors

}
