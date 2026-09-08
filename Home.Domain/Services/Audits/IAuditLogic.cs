using Home.Domain.Entities;

namespace Home.Domain.Services.Audits;

/// <summary>
/// Writes the household's history. Every row carries a plain-English summary of what happened, in
/// the past tense with the doer left off ("ticked off 'Bins'"), because the feed puts the name in
/// front. The logic works the summary out from the entity; an interactor that knows better passes
/// its own.
/// </summary>
public interface IAuditLogic<TEntity> where TEntity : class
{

    #region Methods

    void AddAudit(TEntity entity);

    /// <summary>
    /// Records an addition with a summary the caller composed, for when the entity alone cannot say
    /// what happened ("imported the recipe 'X'" rather than "added the recipe 'X'").
    /// </summary>
    void AddAudit(TEntity entity, string summary);

    /// <summary>
    /// Records that the entity was removed. The history stays: a removal is one more thing that
    /// happened, not a reason to forget the rest.
    /// </summary>
    void DeleteAudit(TEntity entity);

    IQueryable<Audit> GetAudits();

    void UpdateAudit(TEntity entity);

    /// <summary>
    /// Records a change with a summary the caller composed.
    /// </summary>
    void UpdateAudit(TEntity entity, string summary);

    #endregion Methods

}
