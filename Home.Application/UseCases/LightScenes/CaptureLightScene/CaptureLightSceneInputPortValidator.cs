using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.LightScenes.CaptureLightScene;

public class CaptureLightSceneInputPortValidator : BaseValidator<CaptureLightSceneInputPort>
{

    #region Constructors

    public CaptureLightSceneInputPortValidator()
    {
        _ = this.RuleFor(r => r.Name).NotEmpty().MaximumLength(250);
    }

    #endregion Constructors

}
