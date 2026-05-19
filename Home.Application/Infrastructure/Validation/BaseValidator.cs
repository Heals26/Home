using CleanArchitecture.Mediator;
using FluentValidation;
using Home.Application.Services.Validation;

namespace Home.Application.Infrastructure.Validation;

public abstract class BaseValidator<TInputPort> : AbstractValidator<TInputPort>, IInputPortValidator<TInputPort, HomeInputPortValidationFailure>
    where TInputPort : class, IInputPort<IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>>
{

    #region Methods

    Task<bool> IInputPortValidator<TInputPort, HomeInputPortValidationFailure>.ValidateAsync(TInputPort inputPort, out HomeInputPortValidationFailure validationFailure, ServiceFactory serviceFactory, CancellationToken cancellationToken)
    {
        var _Result = this.Validate(inputPort);

        if (_Result.IsValid)
        {
            validationFailure = new([]);
            return Task.FromResult(true);
        }

        validationFailure = new HomeInputPortValidationFailure(
            _Result.Errors.Select(e => new HomeInputPortValidationFailure.ValidationError(e.PropertyName, e.ErrorMessage)).ToList());

        return Task.FromResult(false);
    }

    #endregion Methods

}
