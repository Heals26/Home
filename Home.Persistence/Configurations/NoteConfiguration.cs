using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{

    #region Methods

    public void Configure(EntityTypeBuilder<Note> entity)
    {
        _ = entity.ToTable(nameof(Note), DomainValues.Schema);

        _ = entity.HasKey(e => e.NoteID);
        _ = entity.Property(e => e.NoteID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Content)
            .IsRequired();

        // The database stamps the row, not the model: a CLR-evaluated default is frozen at
        // scaffold time, which both stamped every defaulted Note with the same stale moment and
        // made every later migration re-alter this column as the "default" moved.
        _ = entity.Property(e => e.CreatedOnUTC)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        _ = entity.Ignore(e => e.Audits);
    }

    #endregion Methods

}
