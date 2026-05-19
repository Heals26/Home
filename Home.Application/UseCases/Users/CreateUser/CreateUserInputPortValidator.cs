using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.Users.CreateUser;

public class CreateUserInputPortValidator : BaseValidator<CreateUserInputPort>
{

    #region Constructors

    public CreateUserInputPortValidator()
    {
        _ = this.RuleFor(r => r.Email).EmailAddress().MaximumLength(500);
        _ = this.RuleFor(r => r.FirstName).NotEmpty().MaximumLength(50);
        _ = this.RuleFor(r => r.LastName).NotEmpty().MaximumLength(50);
        _ = this.RuleFor(r => r.MiddleNames).MaximumLength(50);
        _ = this.RuleFor(r => r.Password).NotEmpty();
    }

    #endregion Constructors

}
