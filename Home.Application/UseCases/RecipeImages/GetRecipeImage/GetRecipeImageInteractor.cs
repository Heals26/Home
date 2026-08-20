using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeImages.GetRecipeImage;

internal class GetRecipeImageInteractor : IInteractor<GetRecipeImageInputPort, IGetRecipeImageOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetRecipeImageInputPort inputPort,
        IGetRecipeImageOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Image = _PersistenceContext.GetEntities<RecipeImage>()
            .Where(i => i.Recipe.RecipeID == inputPort.RecipeID
                && i.Recipe.Household.HouseholdID == _Household.HouseholdID)
            .Select(i => new { i.Content, i.ContentType })
            .SingleOrDefault();

        if (_Image == null)
        {
            await outputPort.PresentRecipeImageNotFoundAsync(inputPort.RecipeID, cancellationToken);
            return;
        }

        await outputPort.PresentRecipeImageAsync(_Image.Content, _Image.ContentType, cancellationToken);
    }

    #endregion Methods

}
