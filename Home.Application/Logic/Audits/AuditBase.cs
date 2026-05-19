using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Services.Audits;
using Microsoft.EntityFrameworkCore;

namespace Home.Application.Logic.Audits;

public abstract class AuditBase<TEntity>(IAuthorisationService authorisationService, IPersistenceContext persistenceContext)
    : IAuditLogic<TEntity> where TEntity : class
{

    #region Methods

    protected abstract void AddEntity(TEntity entity);

    protected abstract void DeleteEntity(TEntity entity);

    protected abstract void UpdateEntity(TEntity entity);

    void IAuditLogic<TEntity>.AddAudit(TEntity entity)
        => this.AddEntity(entity);

    void IAuditLogic<TEntity>.DeleteAudit(TEntity entity)
        => this.DeleteEntity(entity);

    void IAuditLogic<TEntity>.UpdateAudit(TEntity entity)
        => this.UpdateEntity(entity);

    protected abstract IQueryable<Audit> GetAudits();

    IQueryable<Audit> IAuditLogic<TEntity>.GetAudits()
        => this.GetAudits();

    protected virtual string GetAuditChanges(TEntity entity, EntityState entityState)
    {
        var _Changes = new List<string>();

        switch (entityState)
        {
            case EntityState.Added:
                _Changes.Add($"Add: {this.SerializeEntityValues(entity)}");
                break;
            case EntityState.Modified:
                var _Entry = persistenceContext.Entity(entity);
                foreach (var _Property in _Entry.Properties)
                    if (_Property.IsModified)
                        _Changes.Add($"Property: '{_Property.Metadata.Name}' changed from '{_Property.OriginalValue}' to '{_Property.CurrentValue}'");
                break;
            case EntityState.Deleted:
                _Changes.Add($"Deleted: {this.GetEntityID(entity)}");
                break;
            default:
                break;
        }

        return string.Join(", ", _Changes);
    }

    protected virtual User GetUser()
        => authorisationService.GetUser();

    private object GetEntityID(TEntity entity)
    {
        var _PropertyID = entity.GetType().GetProperty($"{typeof(TEntity).Name}ID");
        return _PropertyID!.GetValue(entity)!;
    }

    private string SerializeEntityValues(TEntity entity)
    {
        var _Properties = entity.GetType().GetProperties()
            .Select(p => $"{p.Name}: {p.GetValue(entity)}");
        return string.Join(", ", _Properties);
    }

    #endregion Methods

}
