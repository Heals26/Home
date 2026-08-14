using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeSteps.GetRecipeStep;

internal class GetRecipeStepInteractor : IInteractor<GetRecipeStepInputPort, IGetRecipeStepOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetRecipeStepInputPort inputPort,
        IGetRecipeStepOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _PersistenceContext = serviceFactory.GetService<IPersistenceContext>();
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();

        var _Household = _AuthorisationService.GetHousehold();

        var _Step = _PersistenceContext.GetEntities<Recipe>()
            .Where(r => r.Household.HouseholdID == _Household.HouseholdID)
            .SelectMany(r => r.Steps)
            .SingleOrDefault(s => s.RecipeStepID == inputPort.RecipeStepID);

        if (_Step == null)
            await outputPort.PresentRecipeStepNotFoundAsync(inputPort.RecipeStepID, cancellationToken);
        else
            await outputPort.PresentRecipeStepAsync(_Step, cancellationToken);
    }

    #endregion Methods

}
