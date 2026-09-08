using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

public class ShoppingListAuditLogic(
    IAuthorisationService authorisationService,
    IPersistenceContext persistenceContext,
    TimeProvider timeProvider)
    : AuditBase<ShoppingList>(authorisationService, persistenceContext, timeProvider)
{

    #region Properties

    protected override ResourceTypeSE ResourceType => ResourceTypeSE.ShoppingCart;

    #endregion Properties

    #region Methods

    protected override string Describe(ShoppingList shoppingList, EntityState entityState)
    {
        var _Name = Quote(shoppingList.Name);

        if (entityState == EntityState.Added)
            return $"started the list {_Name}";

        if (entityState == EntityState.Deleted)
            return $"removed the list {_Name}";

        if (this.HasChanged(shoppingList, nameof(ShoppingList.Name)))
            return $"renamed {Quote(this.OriginalValue(shoppingList, nameof(ShoppingList.Name)) as string)} to {_Name}";

        return $"changed the list {_Name}";
    }

    #endregion Methods

}
