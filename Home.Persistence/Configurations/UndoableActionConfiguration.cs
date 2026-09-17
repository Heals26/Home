using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class UndoableActionConfiguration : IEntityTypeConfiguration<UndoableAction>
{

    #region Methods

    public void Configure(EntityTypeBuilder<UndoableAction> entity)
    {
        _ = entity.ToTable(nameof(UndoableAction), DomainValues.Schema);

        _ = entity.HasKey(e => e.UndoableActionID);
        _ = entity.Property(e => e.UndoableActionID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.CanUndo)
            .IsRequired();

        _ = entity.Property(e => e.Changes)
            .IsRequired();

        _ = entity.Property(e => e.CreatedOnUTC)
            .IsRequired();

        _ = entity.Property(e => e.Token)
            .IsRequired();

        _ = entity.Property(e => e.UndoneOnUTC)
            .IsRequired(false);

        _ = entity.Property<long>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany()
            .HasForeignKey("HouseholdID")
            .HasConstraintName("FK_UndoableAction_Household")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        _ = entity.HasIndex(e => e.Token)
            .IsUnique();

        _ = entity.HasIndex(e => e.CreatedOnUTC);
    }

    #endregion Methods

}
