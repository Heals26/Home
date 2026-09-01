using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.CardSections.CreateCardSection;

public class CreateCardSectionInputPortValidator : BaseValidator<CreateCardSectionInputPort>
{

    #region Constructors

    public CreateCardSectionInputPortValidator()
    {
        _ = this.RuleFor(r => r.Name).NotEmpty().MaximumLength(100);
    }

    #endregion Constructors

}