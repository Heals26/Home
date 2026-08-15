using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class MealSlotConfiguration : IEntityTypeConfiguration<MealSlot>
{

    #region Methods

    public void Configure(EntityTypeBuilder<MealSlot> entity)
    {
        _ = entity.ToTable(nameof(MealSlot), DomainValues.Schema);

        _ = entity.HasKey(e => e.MealSlotID);
        _ = entity.Property(e => e.MealSlotID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Sequence);
        _ = entity.Property(e => e.StartsAt)
            .IsRequired(false);

        _ = entity.Property(e => e.Name)
            .HasMaxLength(50)
            .IsRequired();

        _ = entity.Property<long>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany(e => e.MealSlots)
            .HasForeignKey("HouseholdID")
            .HasConstraintName("FK_MealSlot_Household")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        _ = entity.HasIndex("HouseholdID", nameof(MealSlot.Name))
            .IsUnique();
    }

    #endregion Methods

}
