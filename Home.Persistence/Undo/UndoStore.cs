using Home.Application.Services.Persistence;
using Home.Application.Services.Undo;
using Home.Domain.Deletions;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Home.Persistence.Undo;

/// <summary>
/// Works on the rows a request held back or changed, which is below anything a use case should see:
/// deleted rows that every query filters out, reached by type and key rather than by a query of its
/// own.
/// </summary>
public class UndoStore(IPersistenceContext persistenceContext, TimeProvider timeProvider) : IUndoStore
{

    #region Fields

    private static readonly MethodInfo s_DeletedBefore = typeof(UndoStore).GetMethod(nameof(DeletedBefore), BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly MethodInfo s_FindIncludingDeleted = typeof(UndoStore).GetMethod(nameof(FindIncludingDeleted), BindingFlags.NonPublic | BindingFlags.Instance)!;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Saving through the context itself rather than <see cref="IPersistenceContext"/> is deliberate:
    /// nothing done here is held back or written down for an undo of its own.
    /// </summary>
    private DbContext Context
        => (DbContext)persistenceContext;

    #endregion Properties

    #region Methods

    private List<object> DeletedBefore<TEntity>(DateTime beforeUTC) where TEntity : class
        => [.. this.Context.Set<TEntity>()
            .IgnoreQueryFilters()
            .Where(e => EF.Property<DateTime?>(e, nameof(ISoftDeletable.DeletedOnUTC)) < beforeUTC)];

    /// <summary>
    /// Every kind of row that can be held back, each ahead of any kind it depends on, so a row is
    /// deleted before the row it points at and a foreign key that does not cascade is never in the
    /// way.
    /// </summary>
    private static List<IEntityType> DependentsFirst(IModel model)
    {
        var _Remaining = model.GetEntityTypes().Where(t => typeof(ISoftDeletable).IsAssignableFrom(t.ClrType)).ToList();
        var _Ordered = new List<IEntityType>();

        while (_Remaining.Count > 0)
        {
            var _Next = _Remaining.FirstOrDefault(t => !_Remaining.Any(other => other != t
                && other.GetForeignKeys().Any(fk => fk.PrincipalEntityType == t)))
                ?? _Remaining[0];

            _Ordered.Add(_Next);
            _ = _Remaining.Remove(_Next);
        }

        return _Ordered;
    }

    private TEntity? FindIncludingDeleted<TEntity>(IEntityType entityType, IReadOnlyList<UndoValue> key) where TEntity : class
    {
        var _Row = Expression.Parameter(typeof(TEntity), "e");
        Expression? _Match = null;

        foreach (var _Part in key)
        {
            var _Type = entityType.FindProperty(_Part.Property)!.ClrType;

            var _Equal = Expression.Equal(
                Expression.Call(typeof(EF), nameof(EF.Property), [_Type], _Row, Expression.Constant(_Part.Property)),
                Expression.Constant(_Part.Value.Deserialize(_Type), _Type));

            _Match = _Match == null ? _Equal : Expression.AndAlso(_Match, _Equal);
        }

        return this.Context.Set<TEntity>()
            .IgnoreQueryFilters()
            .SingleOrDefault(Expression.Lambda<Func<TEntity, bool>>(_Match!, _Row));
    }

    async Task<int> IUndoStore.PurgeAsync(DateTime beforeUTC, CancellationToken cancellationToken)
    {
        var _Failures = 0;

        foreach (var _EntityType in DependentsFirst(this.Context.Model))
        {
            try
            {
                var _Rows = (List<object>)s_DeletedBefore.MakeGenericMethod(_EntityType.ClrType).Invoke(this, [beforeUTC])!;

                if (_Rows.Count == 0)
                    continue;

                this.Context.RemoveRange(_Rows);
                _ = await this.Context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Left held back and tried again next time, which is still gone as far as anyone can see.
                this.Context.ChangeTracker.Clear();
                _Failures++;
            }
        }

        this.Context.RemoveRange(this.Context.Set<UndoableAction>().Where(a => a.CreatedOnUTC < beforeUTC).ToList());
        _ = await this.Context.SaveChangesAsync(cancellationToken);

        return _Failures;
    }

    async Task<UndoOutcome> IUndoStore.UndoAsync(long householdID, Guid token, DateTime notBeforeUTC, CancellationToken cancellationToken)
    {
        var _Action = this.Context.Set<UndoableAction>()
            .SingleOrDefault(a => a.Token == token
                && a.Household.HouseholdID == householdID
                && a.UndoneOnUTC == null);

        if (_Action == null)
            return UndoOutcome.NotFound;

        if (_Action.CreatedOnUTC < notBeforeUTC)
            return UndoOutcome.Expired;

        if (!_Action.CanUndo)
            return UndoOutcome.CannotUndo;

        foreach (var _Change in Enumerable.Reverse(JsonSerializer.Deserialize<List<UndoChange>>(_Action.Changes) ?? []))
        {
            var _EntityType = this.Context.Model.FindEntityType(_Change.EntityType);

            if (_EntityType == null)
                continue;

            var _Entity = s_FindIncludingDeleted.MakeGenericMethod(_EntityType.ClrType).Invoke(this, [_EntityType, _Change.Key]);

            if (_Entity == null)
                continue;

            if (_Change.Kind == UndoChangeKind.Added)
            {
                _ = this.Context.Remove(_Entity);
                continue;
            }

            // Only what is still as the request left it goes back, so a later change made anywhere else
            // is not overwritten.
            foreach (var _Value in _Change.Values)
            {
                var _Property = this.Context.Entry(_Entity).Property(_Value.Property);
                var _Type = _Property.Metadata.ClrType;

                if (Equals(_Property.CurrentValue, _Value.After.Deserialize(_Type)))
                    _Property.CurrentValue = _Value.Before.Deserialize(_Type);
            }
        }

        _Action.UndoneOnUTC = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            _ = await this.Context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Something has taken the place of what would come back, such as a new photo for a recipe
            // whose old one would return beside it.
            return UndoOutcome.CannotUndo;
        }

        return UndoOutcome.Undone;
    }

    #endregion Methods

}
