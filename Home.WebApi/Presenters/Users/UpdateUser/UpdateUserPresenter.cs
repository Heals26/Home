using AutoMapper;
using CleanArchitecture.Mediator;
using Home.Application.UseCases.Users.UpdateUser;
using Home.WebApi.Infrastructure.Presenters;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Home.WebApi.Presenters.Users.UpdateUser;

public class UpdateUserPresenter(IMapper mapper) : OutputPortPresenter(mapper), IUpdateUserOutputPort
{

    #region Methods

    Task IUpdateUserOutputPort.PresentLoginNeedsEmailAsync(CancellationToken cancellationToken)
        => this.UnprocessableContent(
            new ValidationProblemDetails()
            {
                Detail = "Give the member an email to sign in with before setting a password.",
                Status = (int)HttpStatusCode.UnprocessableContent,
                Title = "A login needs an email.",
                Errors = new Dictionary<string, string[]>()
            },
            cancellationToken);

    Task<ContinuationBehaviour> IUpdateUserOutputPort.PresentUserConflictAsync(string email, CancellationToken cancellationToken)
        => this.ConflictAsync(cancellationToken);

    Task IUpdateUserOutputPort.PresentUserNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
