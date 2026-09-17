using Home.Application.Services.Persistence;
using Home.Application.Services.Undo;
using Home.Domain.Deletions;
using Home.Domain.Entities;
using Home.Persistence.Undo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Text.Json;

namespace Home.Persistence.Database;

public class PersistenceContext(
    DbContextOptions<PersistenceContext> options,
    IUndoScope undoScope,
    TimeProvider timeProvider) : DbContext(options), IPersistenceContext
{

    #region Fields

    /// <summary>
    /// Shared by every list, so another request may already be filing or pricing against a memory by
    /// the time an undo of the request that created it arrives.
    /// </summary>
    private static readonly HashSet<Type> s_KeptOnUndo = [typeof(ShoppingItemMemory)];

    private readonly List<UndoChange> m_UndoChanges = [];
    private UndoableAction? m_UndoableAction;
    private bool m_UndoIsIncomplete;

    #endregion Fields

    #region Methods

    void IPersistenceContext.Add<TEntity>(TEntity entity)
        => base.Add(entity);

    void IPersistenceContext.AddRange<TEntity>(ICollection<TEntity> entities)
        => base.AddRange(entities);

    bool IPersistenceContext.DoesEntityExist<TEntity>(long entityID)
        => base.Find<TEntity>([entityID]) != null;

    EntityEntry IPersistenceContext.Entity<TEntity>(TEntity entity)
        => base.Entry(entity);

    TEntity? IPersistenceContext.Find<TEntity>(object entityID, params object[] additionalEntityIDs) where TEntity : class
        => base.Find<TEntity>([entityID, .. additionalEntityIDs]);

    IQueryable<TEntity> IPersistenceContext.GetEntities<TEntity>()
        => this.Set<TEntity>().AsQueryable();

    /// <summary>
    /// Turns this save's deletes into ones an undo can take back, and writes down what the save
    /// changes. The rows it adds are handed back, because their keys only exist once they are saved.
    /// </summary>
    private List<EntityEntry> HoldBackForUndo()
    {
        var _NowUTC = timeProvider.GetUtcNow().UtcDateTime;
        var _Entries = this.ChangeTracker.Entries().Where(e => e.Entity is not UndoableAction).ToList();
        var _HeldBack = _Entries.Where(e => e.State == EntityState.Deleted && e.Entity is ISoftDeletable).ToList();

        foreach (var _Entry in _HeldBack)
        {
            _Entry.State = EntityState.Unchanged;
            _Entry.Property(nameof(ISoftDeletable.DeletedOnUTC)).CurrentValue = _NowUTC;

            this.m_UndoChanges.Add(new(
                UndoChangeKind.Deleted,
                _Entry.Metadata.Name,
                KeyOf(_Entry),
                [new(nameof(ISoftDeletable.DeletedOnUTC), Serialize(null, typeof(DateTime?)), Serialize(_NowUTC, typeof(DateTime?)))]));
        }

        // A row the delete carried off with one held back stays as well. No query can reach it past
        // its parent, and the purge's real delete takes it with that parent.
        foreach (var _Entry in _Entries.Where(e => e.State == EntityState.Deleted))
        {
            if (_HeldBack.Any(p => IsPrincipalOf(p, _Entry)))
                _Entry.State = EntityState.Unchanged;
            else
                this.m_UndoIsIncomplete = true;
        }

        foreach (var _Entry in _Entries.Where(e => e.State == EntityState.Modified && !_HeldBack.Contains(e)))
        {
            List<UndoValueChange> _Values =
            [
                .. _Entry.Properties
                    .Where(p => p.IsModified && !p.Metadata.IsPrimaryKey())
                    .Select(p => new UndoValueChange(p.Metadata.Name, Serialize(p.OriginalValue, p.Metadata.ClrType), Serialize(p.CurrentValue, p.Metadata.ClrType)))
            ];

            if (_Values.Count > 0)
                this.m_UndoChanges.Add(new(UndoChangeKind.Modified, _Entry.Metadata.Name, KeyOf(_Entry), _Values));
        }

        return [.. _Entries.Where(e => e.State == EntityState.Added && !s_KeptOnUndo.Contains(e.Metadata.ClrType))];
    }

    private static bool IsPrincipalOf(EntityEntry principal, EntityEntry dependent)
        => dependent.Metadata.GetForeignKeys()
            .Where(fk => fk.PrincipalEntityType.IsAssignableFrom(principal.Metadata))
            .Any(fk => fk.Properties.Select(p => dependent.Property(p.Name).CurrentValue)
                .SequenceEqual(fk.PrincipalKey.Properties.Select(p => principal.Property(p.Name).CurrentValue)));

    private static List<UndoValue> KeyOf(EntityEntry entry)
        => [.. entry.Metadata.FindPrimaryKey()!.Properties.Select(p => new UndoValue(p.Name, Serialize(entry.Property(p.Name).CurrentValue, p.ClrType)))];

    /// <summary>
    /// Every row that a delete can hold back is filtered out of every query while it is held back. A
    /// configuration that already filters its rows has said how it hides one of its own.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.ApplyConfigurationsFromAssembly(AssemblyUtility.GetAssembly());

        foreach (var _EntityType in modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(ISoftDeletable).IsAssignableFrom(t.ClrType) && t.GetQueryFilter() == null)
            .ToList())
        {
            var _Row = Expression.Parameter(_EntityType.ClrType, "e");

            _EntityType.SetQueryFilter(Expression.Lambda(
                Expression.Equal(
                    Expression.Property(_Row, nameof(ISoftDeletable.DeletedOnUTC)),
                    Expression.Constant(null, typeof(DateTime?))),
                _Row));
        }
    }

    private static long ReadID(object subject)
    {
        var _Property = subject.GetType().GetProperty($"{subject.GetType().Name}ID");

        return _Property?.GetValue(subject) is long _ID ? _ID : 0;
    }

    void IPersistenceContext.Remove<TEntity>(TEntity entity)
        => base.Remove(entity);

    void IPersistenceContext.RemoveRange<TEntity>(IEnumerable<TEntity> entities)
        => base.RemoveRange(entities);

    /// <summary>
    /// Saves, then gives any history row written for a brand-new entity the ID the database just
    /// assigned. The row is written before the insert, when the entity has no ID, so without this a
    /// thing's own history could never find its first entry.
    /// <para>
    /// A request made with an undo token has its deletes held back and everything it changes written
    /// down against that token, across however many saves it makes.
    /// </para>
    /// </summary>
    async Task<int> IPersistenceContext.SaveChangesAsync(CancellationToken cancellationToken)
    {
        var _Undoable = undoScope.Token != null && undoScope.HouseholdID != null;
        var _Added = _Undoable ? this.HoldBackForUndo() : [];

        var _Pending = this.ChangeTracker.Entries<Audit>()
            .Where(e => e.State == EntityState.Added && e.Entity.Subject != null)
            .Select(e => e.Entity)
            .ToList();

        var _Count = await base.SaveChangesAsync(cancellationToken);

        if (_Undoable)
            this.m_UndoChanges.AddRange(_Added.Select(e => new UndoChange(UndoChangeKind.Added, e.Metadata.Name, KeyOf(e), [])));

        if (_Pending.Count > 0)
        {
            foreach (var _Audit in _Pending)
            {
                _Audit.EntityID = ReadID(_Audit.Subject!);
                _Audit.Subject = null;
            }

            _Count += await base.SaveChangesAsync(cancellationToken);
        }

        if (_Undoable)
            await this.SaveUndoableActionAsync(cancellationToken);

        return _Count;
    }

    private async Task SaveUndoableActionAsync(CancellationToken cancellationToken)
    {
        if (this.m_UndoableAction == null)
        {
            this.m_UndoableAction = new()
            {
                CreatedOnUTC = timeProvider.GetUtcNow().UtcDateTime,
                Token = undoScope.Token!.Value
            };

            _ = this.Add(this.m_UndoableAction);
            this.Entry(this.m_UndoableAction).Property("HouseholdID").CurrentValue = undoScope.HouseholdID!.Value;
        }

        this.m_UndoableAction.CanUndo = !this.m_UndoIsIncomplete;
        this.m_UndoableAction.Changes = JsonSerializer.Serialize(this.m_UndoChanges);

        _ = await base.SaveChangesAsync(cancellationToken);
    }

    private static JsonElement Serialize(object? value, Type type)
        => JsonSerializer.SerializeToElement(value, type);

    #endregion Methods

}
