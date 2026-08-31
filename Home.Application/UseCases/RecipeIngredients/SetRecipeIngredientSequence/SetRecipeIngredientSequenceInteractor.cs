using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeIngredients.SetRecipeIngredientSequence;

internal class SetRecipeIngredientSequenceInteractor : IInteractor<SetRecipeIngredientSequenceInputPort, ISetRecipeIngredientSequenceOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        SetRecipeIngredientSequenceInputPort inputPort,
        ISetRecipeIngredientSequenceOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _RecipeIngredient = _PersistenceContext.GetEntities<RecipeIngredient>()
            .Where(ri => ri.IngredientID == inputPort.IngredientID
                && ri.RecipeID == inputPort.RecipeID
                && ri.Recipe.Household.HouseholdID == _Household.HouseholdID)
            .SingleOrDefault();

        if (_RecipeIngredient == null)
        {
            await outputPort.PresentRecipeIngredientNotFoundAsync(inputPort.IngredientID, cancellationToken);
        }
        else
        {
            _RecipeIngredient.Sequence = inputPort.Sequence;

            _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

            await outputPort.PresentRecipeIngredientSequenceSetAsync(cancellationToken);
        }
    }

    #endregion Methods

}
