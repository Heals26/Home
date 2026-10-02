using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.Support.CreateFeedback;

/// <summary>
/// Who sent it, from where and when all come from the request's own session, never from the web
/// app. No audit row is written, because feedback is not something the household did.
/// </summary>
internal class CreateFeedbackInteractor : IInteractor<CreateFeedbackInputPort, ICreateFeedbackOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        CreateFeedbackInputPort inputPort,
        ICreateFeedbackOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _TimeProvider = serviceFactory.GetService<TimeProvider>();

        var _User = _AuthorisationService.GetUser();
        var _SessionID = _AuthorisationService.GetAuthenticationMetadata().AuthenticationMetadataID;

        var _Feedback = new Feedback()
        {
            Content = inputPort.Content.Trim(),
            CreatedOnUTC = _TimeProvider.GetUtcNow().UtcDateTime,
            DeviceLabel = _PersistenceContext.GetEntities<UserAuthentication>()
                .Where(a => a.AuthenticationMetadataID == _SessionID && a.User.UserID == _User.UserID)
                .Select(a => a.DeviceLabel)
                .SingleOrDefault(),
            Household = _AuthorisationService.GetHousehold(),
            Kind = inputPort.Kind,
            Page = string.IsNullOrWhiteSpace(inputPort.Page) ? null : inputPort.Page.Trim(),
            ScreenHeight = inputPort.ScreenHeight,
            ScreenWidth = inputPort.ScreenWidth,
            User = _User
        };

        _PersistenceContext.Add(_Feedback);
        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentFeedbackCreatedAsync(_Feedback.FeedbackID, cancellationToken);
    }

    #endregion Methods

}
