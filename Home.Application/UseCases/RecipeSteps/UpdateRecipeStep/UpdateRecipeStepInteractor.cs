using CleanArchitecture.Mediator;
using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeSteps.UpdateRecipeStep;

internal class UpdateRecipeStepInteractor : IInteractor<UpdateRecipeStepInputPort, IUpdateRecipeStepOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        UpdateRecipeStepInputPort inputPort,
        IUpdateRecipeStepOutputPort outputPort,
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
        {
            await outputPort.PresentRecipeStepNotFoundAsync(inputPort.RecipeStepID, cancellationToken);
            return;
        }

        if (inputPort.Content.HasBeenSet)
            _Step.Content = inputPort.Content.Value;

        if (inputPort.Sequence.HasBeenSet)
            _Step.Sequence = inputPort.Sequence.Value;

        if (inputPort.Title.HasBeenSet)
            _Step.Title = inputPort.Title.Value;

        _ = await _PersistenceContext.SaveChangesAsync(cancellationToken);

        await outputPort.PresentRecipeStepNoContentAsync(cancellationToken);
    }

    #endregion Methods

}
