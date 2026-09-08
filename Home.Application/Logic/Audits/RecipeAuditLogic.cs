using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

public class RecipeAuditLogic(
    IAuthorisationService authorisationService,
    IPersistenceContext persistenceContext,
    TimeProvider timeProvider)
    : AuditBase<Recipe>(authorisationService, persistenceContext, timeProvider)
{

    #region Properties

    protected override ResourceTypeSE ResourceType => ResourceTypeSE.Recipe;

    #endregion Properties

    #region Methods

    protected override string Describe(Recipe recipe, EntityState entityState)
    {
        var _Name = Quote(recipe.Name);

        if (entityState == EntityState.Added)
            return $"added the recipe {_Name}";

        if (entityState == EntityState.Deleted)
            return $"removed the recipe {_Name}";

        if (this.HasChanged(recipe, nameof(Recipe.Name)))
            return $"renamed {Quote(this.OriginalValue(recipe, nameof(Recipe.Name)) as string)} to {_Name}";

        return $"changed the recipe {_Name}";
    }

    #endregion Methods

}
