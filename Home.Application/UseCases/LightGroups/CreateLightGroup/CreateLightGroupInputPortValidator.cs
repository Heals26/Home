using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.LightGroups.CreateLightGroup;

public class CreateLightGroupInputPortValidator : BaseValidator<CreateLightGroupInputPort>
{

    #region Constructors

    public CreateLightGroupInputPortValidator()
    {
        _ = this.RuleFor(r => r.Name).NotEmpty().MaximumLength(250);
    }

    #endregion Constructors

}
