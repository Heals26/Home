using FluentValidation;
using Home.Application.Infrastructure.Validation;

namespace Home.Application.UseCases.CalendarSubscriptions.CreateCalendarSubscription;

public class CreateCalendarSubscriptionInputPortValidator : BaseValidator<CreateCalendarSubscriptionInputPort>
{

    #region Constructors

    public CreateCalendarSubscriptionInputPortValidator()
    {
        _ = this.RuleFor(r => r.Name)
            .MaximumLength(100);

        _ = this.RuleFor(r => r.Url)
            .NotEmpty()
            .MaximumLength(2000)
            .Must(BeAFeedAddress)
            .WithMessage("Paste the calendar's full address, starting with https:// or webcal://.");
    }

    #endregion Constructors

    #region Methods

    private static bool BeAFeedAddress(string url)
        => Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var _Uri)
            && (_Uri.Scheme == Uri.UriSchemeHttps || _Uri.Scheme == Uri.UriSchemeHttp || _Uri.Scheme == "webcal");

    #endregion Methods

}
