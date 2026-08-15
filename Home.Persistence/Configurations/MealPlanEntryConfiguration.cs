using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class MealPlanEntryConfiguration : IEntityTypeConfiguration<MealPlanEntry>
{

    #region Methods

    public void Configure(EntityTypeBuilder<MealPlanEntry> entity)
    {
        _ = entity.ToTable(nameof(MealPlanEntry), DomainValues.Schema);

        _ = entity.HasKey(e => e.MealPlanEntryID);
        _ = entity.Property(e => e.MealPlanEntryID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Date);

        // The recipe is the only owner. Household is reached through it, which keeps SQL Server
        // to one cascade path — deleting a recipe takes its plan entries with it.
        _ = entity.Property<long>("RecipeID");
        _ = entity.HasOne(e => e.Recipe)
            .WithMany()
            .HasForeignKey("RecipeID")
            .HasConstraintName("FK_MealPlanEntry_Recipe")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Restricted, not cascading: the household is already reached through the recipe, and a
        // second cascade path would be rejected. Refusing to delete a slot still holding a week
        // of dinners is also the behaviour the family wants.
        _ = entity.Property<long?>("MealSlotID");
        _ = entity.HasOne(e => e.MealSlot)
            .WithMany()
            .HasForeignKey("MealSlotID")
            .HasConstraintName("FK_MealPlanEntry_MealSlot")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // The planner always reads a date window.
        _ = entity.HasIndex(e => e.Date);
    }

    #endregion Methods

}
