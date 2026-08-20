using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeImages.DeleteRecipeImage;

internal class DeleteRecipeImageInteractor : IInteractor<DeleteRecipeImageInputPort, IDeleteRecipeImageOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        DeleteRecipeImageInputPort inputPort,
        IDeleteRecipeImageOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Recipe = _PersistenceContext.GetEntities<Recipe>()
            .SingleOrDefault(r => r.RecipeID == inputPort.RecipeID
                && r.Household.HouseholdID == _Household.HouseholdID);

        if (_Recipe != null)
        {
            _Recipe.ImageUpdatedOnUTC = null;

            _PersistenceContext.GetEntities<RecipeImage>()
                .Where(i => i.Recipe.RecipeID == inputPort.RecipeID)
                .ToList()
                .ForEach(_PersistenceContext.Remove);

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);
        }

        await outputPort.PresentRecipeImageDeletedNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
