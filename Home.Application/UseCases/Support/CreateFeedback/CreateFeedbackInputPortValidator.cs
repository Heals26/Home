using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.Support.CreateFeedback;

public class CreateFeedbackInputPortValidator : BaseValidator<CreateFeedbackInputPort>
{

    #region Constructors

    public CreateFeedbackInputPortValidator()
    {
        _ = this.RuleFor(r => r.Content)
            .NotEmpty()
            .MaximumLength(2000);

        _ = this.RuleFor(r => r.Kind)
            .IsInEnum();

        _ = this.RuleFor(r => r.Page)
            .MaximumLength(2000);
    }

    #endregion Constructors

}
