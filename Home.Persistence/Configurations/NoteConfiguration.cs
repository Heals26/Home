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
        entity.ToTable(nameof(Note), DomainValues.Schema);

        entity.HasKey(e => e.NoteID);
        entity.Property(e => e.NoteID)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.Content)
            .IsRequired();

        entity.Property(e => e.CreatedOnUTC)
            .HasDefaultValue(DateTime.UtcNow)
            .IsRequired();

        _ = entity.HasMany(e => e.Audits)
                .WithOne()
                .HasForeignKey(e => new { e.AuditID, e.EntityID, e.Entity })
                .HasConstraintName("FK_Note_Audit")
                .OnDelete(DeleteBehavior.Cascade);
    }

    #endregion Methods

}
