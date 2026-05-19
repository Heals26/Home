using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

public class ShoppingListAuditLogic(IAuthorisationService authorisationService, IPersistenceContext persistenceContext)
    : AuditBase<ShoppingList>(authorisationService, persistenceContext)
{

    #region Methods

    protected override void AddEntity(ShoppingList shoppingList)
        => persistenceContext.Add(new Audit()
        {
            ModifiedDateUTC = DateTime.UtcNow,
            User = this.GetUser(),
            Entity = ResourceTypeSE.ShoppingCart,
            EntityID = shoppingList.ShoppingListID,
            Content = this.GetAuditChanges(shoppingList, EntityState.Added),
            UserName = this.GetUser()?.UserName,
        });

    protected override void DeleteEntity(ShoppingList entity)
        => persistenceContext.GetEntities<Audit>()
            .Where(a => a.Entity == ResourceTypeSE.ShoppingCart && a.EntityID == entity.ShoppingListID)
            .ToList()
            .ForEach(a => persistenceContext.Remove(a));

    protected override IQueryable<Audit> GetAudits()
        => persistenceContext.GetEntities<Audit>().Where(a => a.Entity == ResourceTypeSE.Activity);

    protected override void UpdateEntity(ShoppingList shoppingList)
        => persistenceContext.Add(new Audit()
        {
            ModifiedDateUTC = DateTime.UtcNow,
            User = this.GetUser(),
            Entity = ResourceTypeSE.ShoppingCart,
            EntityID = shoppingList.ShoppingListID,
            Content = this.GetAuditChanges(shoppingList, EntityState.Modified),
            UserName = this.GetUser()?.UserName,
        });

    #endregion Methods

}
