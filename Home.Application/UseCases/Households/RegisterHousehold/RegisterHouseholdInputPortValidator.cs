using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.Households.RegisterHousehold;

public class RegisterHouseholdInputPortValidator : BaseValidator<RegisterHouseholdInputPort>
{

    #region Constructors

    public RegisterHouseholdInputPortValidator()
    {
        _ = this.RuleFor(r => r.Email).EmailAddress().MaximumLength(500);
        _ = this.RuleFor(r => r.FirstName).NotEmpty().MaximumLength(50);
        _ = this.RuleFor(r => r.HouseholdName).NotEmpty().MaximumLength(250);
        _ = this.RuleFor(r => r.LastName).NotEmpty().MaximumLength(50);
        _ = this.RuleFor(r => r.Password).NotEmpty();
    }

    #endregion Constructors

}
