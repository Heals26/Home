using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class IngredientNoteConfiguration : IEntityTypeConfiguration<IngredientNote>
{

    #region Methods

    public void Configure(EntityTypeBuilder<IngredientNote> entity)
    {
        _ = entity.ToTable(nameof(IngredientNote), DomainValues.Schema);

        _ = entity.HasKey(e => new { e.NoteID, e.IngredientID });

        _ = entity.Property(e => e.IngredientID);
        _ = entity.HasOne(e => e.Ingredient)
            .WithMany(e => e.Notes)
            .HasConstraintName("FK_IngredientNote_Ingredient")
            .HasForeignKey(e => e.IngredientID)
            .OnDelete(DeleteBehavior.Cascade);

        _ = entity.Property(e => e.NoteID);
        _ = entity.HasOne(e => e.Note)
            .WithMany()
            .HasConstraintName("FK_IngredientNote_Note")
            .HasForeignKey(e => e.NoteID)
            .OnDelete(DeleteBehavior.Cascade);
    }

    #endregion Methods

}
