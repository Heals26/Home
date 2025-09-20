using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

public class ShoppingCartAuditLogic(IAuthorisationService authorisationService, IPersistenceContext persistenceContext)
    : AuditBase<ShoppingCart>(authorisationService, persistenceContext)
{

    #region Methods

    protected override void AddEntity(ShoppingCart shoppingCart)
        => persistenceContext.Add(new Audit()
        {
            ModifiedDateUTC = DateTime.UtcNow,
            User = this.GetUser(),
            Entity = ResourceTypeSE.ShoppingCart,
            EntityID = shoppingCart.ShoppingCartID,
            Content = this.GetAuditChanges(shoppingCart, EntityState.Added),
            UserName = this.GetUser()?.UserName,
        });

    protected override IQueryable<Audit> GetAudits()
        => persistenceContext.GetEntities<Audit>().Where(a => a.Entity == ResourceTypeSE.Activity);

    protected override void UpdateEntity(ShoppingCart shoppingCart)
        => persistenceContext.Add(new Audit()
        {
            ModifiedDateUTC = DateTime.UtcNow,
            User = this.GetUser(),
            Entity = ResourceTypeSE.ShoppingCart,
            EntityID = shoppingCart.ShoppingCartID,
            Content = this.GetAuditChanges(shoppingCart, EntityState.Modified),
            UserName = this.GetUser()?.UserName,
        });

    #endregion Methods

}
