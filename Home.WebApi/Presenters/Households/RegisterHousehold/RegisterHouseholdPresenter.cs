using AutoMapper;
using CleanArchitecture.Mediator;
using Home.Application.UseCases.Households.RegisterHousehold;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Households.RegisterHousehold;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Home.WebApi.Presenters.Households.RegisterHousehold;

public class RegisterHouseholdPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IRegisterHouseholdOutputPort
{

    #region Methods

    Task IRegisterHouseholdOutputPort.PresentHouseholdRegisteredAsync(long householdID, CancellationToken cancellationToken)
        => this.CreatedAsync(householdID, new RegisterHouseholdApiResponse() { HouseholdID = householdID }, cancellationToken);

    /// <summary>
    /// A 422 with the reason in words rather than a bare 409, because the sign-up page shows the
    /// detail and a 409 reaches it as an unexpected response.
    /// </summary>
    Task<ContinuationBehaviour> IRegisterHouseholdOutputPort.PresentUserConflictAsync(string email, CancellationToken cancellationToken)
    {
        _ = this.UnprocessableContent(
            new ValidationProblemDetails()
            {
                Detail = "There is already an account with that email. Sign in with it, or use a different one.",
                Status = (int)HttpStatusCode.UnprocessableContent,
                Title = "That email is already in use.",
                Errors = new Dictionary<string, string[]>()
            },
            cancellationToken);

        return ContinuationBehaviour.ReturnAsync;
    }

    #endregion Methods

}
