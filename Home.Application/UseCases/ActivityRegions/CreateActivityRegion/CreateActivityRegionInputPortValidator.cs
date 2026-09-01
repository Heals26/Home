using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.ActivityRegions.CreateActivityRegion;

public class CreateActivityRegionInputPortValidator : BaseValidator<CreateActivityRegionInputPort>
{

    #region Constructors

    public CreateActivityRegionInputPortValidator()
    {
        _ = this.RuleFor(r => r.ActivityID)
            .GreaterThan(0);

        // Which sections exist is a household question now, not a fixed list, so it is answered by
        // the interactor against that household rather than by a rule here.
        _ = this.RuleFor(r => r.CardSectionID)
            .GreaterThan(0);
    }

    #endregion Constructors

}