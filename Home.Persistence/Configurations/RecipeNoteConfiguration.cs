using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class RecipeNoteConfiguration : IEntityTypeConfiguration<RecipeNote>
{

    #region Methods

    public void Configure(EntityTypeBuilder<RecipeNote> entity)
    {
        _ = entity.ToTable(nameof(RecipeNote), DomainValues.Schema);

        _ = entity.HasKey(e => new { e.NoteID, e.RecipeID });

        _ = entity.Property(e => e.RecipeID);
        _ = entity.HasOne(e => e.Recipe)
            .WithMany(e => e.Notes)
            .HasConstraintName("FK_RecipeNote_Recipe")
            .HasForeignKey(e => e.RecipeID)
            .OnDelete(DeleteBehavior.Cascade);

        _ = entity.Property(e => e.NoteID);
        _ = entity.HasOne(e => e.Note)
            .WithMany()
            .HasConstraintName("FK_RecipeNote_Note")
            .HasForeignKey(e => e.NoteID)
            .OnDelete(DeleteBehavior.Cascade);
    }

    #endregion Methods

}
