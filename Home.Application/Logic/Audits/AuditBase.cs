using Home.Application.Services.Persistence;
using Home.Application.Services.Security;
using Home.Domain.Entities;
using Home.Domain.Enumerations;
using Home.Domain.Services.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Home.Application.Logic.Audits;

/// <summary>
/// The shape every kind of history shares: one row per thing that happened, stamped with who, when
/// and which household, carrying both a plain-English summary for the feed and the technical
/// detail for anyone who needs it. Subclasses say which resource they describe and how to put an
/// event into words.
/// </summary>
public abstract class AuditBase<TEntity>(
    IAuthorisationService authorisationService,
    IPersistenceContext persistenceContext,
    TimeProvider timeProvider)
    : IAuditLogic<TEntity> where TEntity : class
{

    #region Properties

    /// <summary>
    /// Audit timestamps come from here rather than DateTime.UtcNow so they can be fixed in tests.
    /// </summary>
    protected DateTime NowUTC => timeProvider.GetUtcNow().UtcDateTime;

    protected abstract ResourceTypeSE ResourceType { get; }

    #endregion Properties

    #region Methods

    void IAuditLogic<TEntity>.AddAudit(TEntity entity)
        => this.Write(entity, EntityState.Added, this.Describe(entity, EntityState.Added));

    void IAuditLogic<TEntity>.AddAudit(TEntity entity, string summary)
        => this.Write(entity, EntityState.Added, summary);

    void IAuditLogic<TEntity>.DeleteAudit(TEntity entity)
        => this.Write(entity, EntityState.Deleted, this.Describe(entity, EntityState.Deleted));

    IQueryable<Audit> IAuditLogic<TEntity>.GetAudits()
        => persistenceContext.GetEntities<Audit>().Where(a => a.Entity == this.ResourceType);

    void IAuditLogic<TEntity>.UpdateAudit(TEntity entity)
        => this.Write(entity, EntityState.Modified, this.Describe(entity, EntityState.Modified));

    void IAuditLogic<TEntity>.UpdateAudit(TEntity entity, string summary)
        => this.Write(entity, EntityState.Modified, summary);

    /// <summary>
    /// What happened, in the past tense, without the doer: "ticked off 'Bins'". The feed puts the
    /// name in front.
    /// </summary>
    protected abstract string Describe(TEntity entity, EntityState entityState);

    /// <summary>
    /// The technical record of the change, kept alongside the summary.
    /// </summary>
    protected virtual string GetAuditChanges(TEntity entity, EntityState entityState)
    {
        var _Changes = new List<string>();

        switch (entityState)
        {
            case EntityState.Added:
                _Changes.Add($"Add: {this.SerializeEntityValues(entity)}");
                break;
            case EntityState.Modified:
                foreach (var _Property in this.Entry(entity).Properties)
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

    protected virtual Household? GetHousehold()
        => authorisationService.GetHousehold();

    protected virtual User? GetUser()
        => authorisationService.GetUser();

    /// <summary>
    /// Whether any of the named properties (shadow foreign keys included) changed in this unit of
    /// work, which is how a summary tells "ticked off" from "renamed".
    /// </summary>
    protected bool HasChanged(TEntity entity, params string[] propertyNames)
        => this.Entry(entity).Properties.Any(p => p.IsModified && propertyNames.Contains(p.Metadata.Name));

    /// <summary>
    /// The value a property had before this unit of work, or null when it did not change.
    /// </summary>
    protected object? OriginalValue(TEntity entity, string propertyName)
        => this.Entry(entity).Properties.FirstOrDefault(p => p.Metadata.Name == propertyName && p.IsModified)?.OriginalValue;

    protected static string Quote(string? name)
        => $"'{(string.IsNullOrWhiteSpace(name) ? "untitled" : name.Trim())}'";

    private EntityEntry Entry(TEntity entity)
        => persistenceContext.Entity(entity);

    private object GetEntityID(TEntity entity)
    {
        var _PropertyID = entity.GetType().GetProperty($"{typeof(TEntity).Name}ID");
        return _PropertyID!.GetValue(entity)!;
    }

    private string SerializeEntityValues(TEntity entity)
    {
        var _Values = entity.GetType().GetProperties()
            .Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string))
            .Select(p => $"{p.Name}={p.GetValue(entity)}");

        return string.Join(", ", _Values);
    }

    private void Write(TEntity entity, EntityState entityState, string summary)
    {
        var _User = this.GetUser();

        persistenceContext.Add(new Audit()
        {
            Content = Truncate(this.GetAuditChanges(entity, entityState), 1000),
            Entity = this.ResourceType,
            EntityID = Convert.ToInt64(this.GetEntityID(entity)),
            Household = this.GetHousehold(),
            ModifiedDateUTC = this.NowUTC,
            // A new entity has no ID yet; the persistence context fills EntityID in after the insert.
            Subject = entityState == EntityState.Added ? entity : null,
            Summary = Truncate(summary, 250),
            User = _User,
            UserName = _User?.UserName
        });
    }

    private static string Truncate(string value, int length)
        => value.Length <= length ? value : value[..length];

    #endregion Methods

}
