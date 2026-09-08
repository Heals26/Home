using FluentValidation;
using Home.Application.Infrastructure.Validation;
using Home.Application.UseCases.Calendar.Models;

namespace Home.Application.UseCases.Calendar.CreateCalendarEvent;

public class CreateCalendarEventInputPortValidator : BaseValidator<CreateCalendarEventInputPort>
{

    #region Constructors

    public CreateCalendarEventInputPortValidator()
    {
        this.AddEventRules();

        _ = this.RuleForEach(r => r.MemberUserIDs)
            .GreaterThan(0);
    }

    #endregion Constructors

}
