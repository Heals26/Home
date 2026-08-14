using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.Announcements.CreateAnnouncement;

public class CreateAnnouncementInputPortValidator : BaseValidator<CreateAnnouncementInputPort>
{

    #region Constructors

    public CreateAnnouncementInputPortValidator()
    {
        _ = this.RuleFor(r => r.Content)
            .NotEmpty()
            .MaximumLength(500);
    }

    #endregion Constructors

}
