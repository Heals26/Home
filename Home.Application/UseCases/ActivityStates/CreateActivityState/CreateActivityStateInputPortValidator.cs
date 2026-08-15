using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.ActivityStates.CreateActivityState;

public class CreateActivityStateInputPortValidator : BaseValidator<CreateActivityStateInputPort>
{

    #region Constructors

    public CreateActivityStateInputPortValidator()
    {
        _ = this.RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(100);
    }

    #endregion Constructors

}
