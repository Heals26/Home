using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.Users.CreateUser;

public class CreateUserInputPortValidator : BaseValidator<CreateUserInputPort>
{

    #region Constructors

    public CreateUserInputPortValidator()
    {
        _ = this.RuleFor(r => r.Email)
            .EmailAddress()
            .MaximumLength(500)
            .When(r => !string.IsNullOrWhiteSpace(r.Email));

        _ = this.RuleFor(r => r.Email)
            .NotEmpty()
            .When(r => !string.IsNullOrWhiteSpace(r.Password))
            .WithMessage("A password needs an email to sign in with.");

        _ = this.RuleFor(r => r.FirstName).NotEmpty().MaximumLength(50);
        _ = this.RuleFor(r => r.LastName).NotEmpty().MaximumLength(50);
        _ = this.RuleFor(r => r.MiddleNames).MaximumLength(50);

        _ = this.RuleFor(r => r.Password)
            .NotEmpty()
            .When(r => !string.IsNullOrWhiteSpace(r.Email))
            .WithMessage("A member with an email needs a password, or leave both off for someone who will not sign in.");
    }

    #endregion Constructors

}
