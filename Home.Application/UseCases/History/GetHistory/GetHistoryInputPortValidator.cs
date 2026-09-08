using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.History.GetHistory;

public class GetHistoryInputPortValidator : BaseValidator<GetHistoryInputPort>
{

    #region Constructors

    public GetHistoryInputPortValidator()
    {
        _ = this.RuleFor(r => r.Take)
            .InclusiveBetween(1, 100);
    }

    #endregion Constructors

}
