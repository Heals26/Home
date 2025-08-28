using AutoMapper;
using CleanArchitecture.Mediator;
using Home.Application.Services.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Infrastructure.Presenters;

public class OutputPortPresenter : IAuthenticationFailureOutputPort,
    IInputPortValidationFailureOutputPort<HomeInputPortValidationFailure>,
    IAuthorisationPolicyFailureOutputPort<HomeAuthorisationFailure>
{

    #region Fields

    private readonly IMapper? m_Mapper;

    #endregion Fields

    #region Constructors

    public OutputPortPresenter(IMapper mapper)
        => this.m_Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public Task PresentAuthenticationFailureAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<ContinuationBehaviour> PresentInputPortValidationFailureAsync(HomeInputPortValidationFailure validationFailure, CancellationToken cancellationToken) => throw new NotImplementedException();


    #endregion Constructors

    #region - - - - - - Properties - - - - - -

    public bool PresentedSuccessfully { get; private set; }

    public IActionResult Result { get; private set; }

    #endregion Properties

    #region Methods

    protected Task UnauthorisedAsync(HomeAuthorisationFailure failure, CancellationToken cancellation)
    {
        this.Result = new UnauthorizedResult();
        return Task.CompletedTask;
    }

    Task<ContinuationBehaviour> IAuthorisationPolicyFailureOutputPort<HomeAuthorisationFailure>.PresentAuthorisationPolicyFailureAsync(HomeAuthorisationFailure policyFailure, CancellationToken cancellationToken) => throw new NotImplementedException();

    #endregion Methods

    #region OutputPort Generic Methods



    #endregion OutputPort Generic Methods

}

