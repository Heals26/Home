using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

public class ShoppingCategoryAuditLogic(
    IAuthorisationService authorisationService,
    IPersistenceContext persistenceContext,
    TimeProvider timeProvider)
    : AuditBase<ShoppingCategory>(authorisationService, persistenceContext, timeProvider)
{

    #region Properties

    protected override ResourceTypeSE ResourceType => ResourceTypeSE.ShoppingCategory;

    #endregion Properties

    #region Methods

    protected override string Describe(ShoppingCategory shoppingCategory, EntityState entityState)
    {
        var _Name = Quote(shoppingCategory.Name);

        if (entityState == EntityState.Added)
            return $"added the aisle {_Name}";

        if (entityState == EntityState.Deleted)
            return $"removed the aisle {_Name}";

        if (this.HasChanged(shoppingCategory, nameof(ShoppingCategory.Name)))
            return $"renamed the aisle {Quote(this.OriginalValue(shoppingCategory, nameof(ShoppingCategory.Name)) as string)} to {_Name}";

        return $"changed the aisle {_Name}";
    }

    #endregion Methods

}
