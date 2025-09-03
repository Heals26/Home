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

        _ = entity.Property(e => e.CreatedOnUTC)
            .HasDefaultValue(DateTime.UtcNow)
            .IsRequired();

        _ = entity.HasMany(e => e.Audits)
                .WithOne()
            .HasForeignKey(e => e.EntityID)
                .HasConstraintName("FK_Note_Audit")
                .OnDelete(DeleteBehavior.ClientCascade);
    }

    #endregion Methods

}
