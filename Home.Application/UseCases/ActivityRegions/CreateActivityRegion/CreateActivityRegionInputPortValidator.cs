using FluentValidation;
using Home.Application.Infrastructure.Validation;
using Home.Domain.Enumerations;

namespace Home.Application.UseCases.ActivityRegions.CreateActivityRegion;

public class CreateActivityRegionInputPortValidator : BaseValidator<CreateActivityRegionInputPort>
{

    #region Constructors

    public CreateActivityRegionInputPortValidator()
    {
        var _RegionNames = BaseEnumeration.GetAll<RegionSE>().Select(r => r.Name).ToList();

        _ = this.RuleFor(r => r.ActivityID)
            .GreaterThan(0);

        // The conversion to RegionSE throws on an unrecognised name, so it has to be caught here.
        _ = this.RuleFor(r => r.Region)
            .NotEmpty()
            .Must(r => _RegionNames.Contains(r))
            .WithMessage($"Region must be one of: {string.Join(", ", _RegionNames)}.");
    }

    #endregion Constructors

}
