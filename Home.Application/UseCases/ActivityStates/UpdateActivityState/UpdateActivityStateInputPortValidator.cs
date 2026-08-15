using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.ActivityStates.UpdateActivityState;

public class UpdateActivityStateInputPortValidator : BaseValidator<UpdateActivityStateInputPort>
{

    #region Constructors

    public UpdateActivityStateInputPortValidator()
    {
        _ = this.RuleFor(r => r.Name.Value)
            .NotEmpty()
            .MaximumLength(100)
            .When(r => r.Name.HasBeenSet)
            .WithName("Name");

        _ = this.RuleFor(r => r.Sequence.Value)
            .GreaterThanOrEqualTo(0)
            .When(r => r.Sequence.HasBeenSet)
            .WithName("Sequence");
    }

    #endregion Constructors

}
