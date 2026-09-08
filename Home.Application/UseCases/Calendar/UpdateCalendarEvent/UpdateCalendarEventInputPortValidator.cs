using FluentValidation;
using Home.Application.Infrastructure.Validation;
using Home.Application.UseCases.Calendar.Models;

namespace Home.Application.UseCases.Calendar.UpdateCalendarEvent;

public class UpdateCalendarEventInputPortValidator : BaseValidator<UpdateCalendarEventInputPort>
{

    #region Constructors

    public UpdateCalendarEventInputPortValidator()
    {
        this.AddEventRules();

        _ = this.RuleForEach(r => r.MemberUserIDs)
            .GreaterThan(0);
    }

    #endregion Constructors

}
