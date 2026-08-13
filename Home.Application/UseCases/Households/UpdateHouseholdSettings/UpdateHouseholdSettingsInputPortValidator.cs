using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.Households.UpdateHouseholdSettings;

public class UpdateHouseholdSettingsInputPortValidator : BaseValidator<UpdateHouseholdSettingsInputPort>
{

    #region Constructors

    public UpdateHouseholdSettingsInputPortValidator()
    {
        _ = this.RuleFor(r => r.Latitude.Value)
            .InclusiveBetween(-90, 90)
            .When(r => r.Latitude.HasBeenSet && r.Latitude.Value != null)
            .WithName("Latitude");

        // An empty token is a deliberate disconnect, so only the length is checked.
        _ = this.RuleFor(r => r.LifxApiToken.Value)
            .MaximumLength(500)
            .When(r => r.LifxApiToken.HasBeenSet)
            .WithName("LifxApiToken");

        _ = this.RuleFor(r => r.Longitude.Value)
            .InclusiveBetween(-180, 180)
            .When(r => r.Longitude.HasBeenSet && r.Longitude.Value != null)
            .WithName("Longitude");

        _ = this.RuleFor(r => r.Name.Value)
            .NotEmpty()
            .MaximumLength(250)
            .When(r => r.Name.HasBeenSet)
            .WithName("Name");
    }

    #endregion Constructors

}
