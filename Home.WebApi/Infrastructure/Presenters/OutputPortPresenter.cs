using AutoMapper;
using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;
using Home.WebApi.Infrastructure.ObjectResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Home.WebApi.Infrastructure.Presenters;

public class OutputPortPresenter(IMapper mapper) : IAuthenticationFailureOutputPort,
    IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>,
    IAuthorisationPolicyFailureOutputPort<HomeAuthorisationFailure>
{

    #region Properties

    public bool PresentedSuccessfully { get; private set; }

    public IActionResult Result { get; private set; }

    #endregion Properties

    #region Methods
    public Task PresentAuthenticationFailureAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

    Task<ContinuationBehaviour> IAuthorisationPolicyFailureOutputPort<HomeAuthorisationFailure>.PresentAuthorisationPolicyFailureAsync(HomeAuthorisationFailure policyFailure, CancellationToken cancellationToken)
    {
        _ = this.UnauthorisedAsync(policyFailure, cancellationToken);
        return ContinuationBehaviour.ReturnAsync;
    }

    Task<ContinuationBehaviour> IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>.PresentInputPortValidationFailureAsync(HomeInputPortValidationFailure validationFailure, CancellationToken cancellationToken)
    {
        var _Details = new ValidationProblemDetails
        {
            Detail = "See Errors property for more details.",
            Status = (int)HttpStatusCode.UnprocessableContent,
            Title = "One or more properties is invalid.",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            Errors = validationFailure.GetValidationErrors() ?? []
        };

        _ = this.UnprocessableContent(_Details, cancellationToken);
        return ContinuationBehaviour.ReturnAsync;
    }

    #endregion Methods

    #region OutputPort Generic Methods

    protected Task<ContinuationBehaviour> ConflictAsync(CancellationToken cancellationToken)
    {
        this.PresentedSuccessfully = true;
        this.Result = new ConflictResult();

        return ContinuationBehaviour.ReturnAsync;
    }

    protected Task CreatedAsync<TResult>(long resourceID, TResult result, CancellationToken cancellationToken)
    {
        this.PresentedSuccessfully = true;
        this.Result = new CreatedResult();

        return Task.CompletedTask;
    }

    protected Task NoContentAsync(CancellationToken cancellationToken)
    {
        this.PresentedSuccessfully = true;
        this.Result = new NoContentResult();

        return Task.CompletedTask;
    }

    protected Task NotFoundAsync(string errorMessage, CancellationToken cancellationToken)
    {
        this.Result = new NotFoundObjectResult(new ProblemDetails()
        {
            Detail = errorMessage,
            Status = (int)HttpStatusCode.NotFound,
            Title = "Entity was not found.",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
        });

        return Task.CompletedTask;
    }

    protected Task OkAsync<TResult>(TResult result, CancellationToken cancellationToken)
    {
        this.PresentedSuccessfully = true;
        this.Result = new OkObjectResult(result);
        return Task.CompletedTask;
    }

    protected Task OkAsync(Stream stream, string contentType, CancellationToken cancellationToken, string fileName = null)
    {
        this.PresentedSuccessfully = true;
        this.Result = new HomeStreamResult(stream, contentType) { FileName = fileName };

        return Task.CompletedTask;
    }

    protected Task UnauthorisedAsync(HomeAuthorisationFailure failure, CancellationToken cancellation)
    {
        this.Result = new UnauthorizedResult();
        return Task.CompletedTask;
    }

    protected Task UnprocessableContent(ValidationProblemDetails problemDetails, CancellationToken cancellationToken)
    {
        this.Result = new UnprocessableEntityObjectResult(problemDetails);
        return Task.CompletedTask;
    }

    #endregion OutputPort Generic Methods

}
