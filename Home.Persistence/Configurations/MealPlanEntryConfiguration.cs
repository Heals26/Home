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

        _ = entity.Property(e => e.Title)
            .HasMaxLength(250)
            .IsRequired(false);

        // While a recipe's delete can still be undone, its planned meals are hidden with it, the way
        // the cascade below takes them when the delete is carried out.
        _ = entity.HasQueryFilter(e => e.DeletedOnUTC == null
            && (EF.Property<long?>(e, "RecipeID") == null || e.Recipe != null));

        // The household owns the entry outright, because an entry that is only a title has no
        // recipe to be reached through. Restricted rather than cascading: the recipe below already
        // cascades and SQL Server will not accept two cascade paths to the same table. Nothing in
        // the application deletes a household, so the restriction costs nothing.
        _ = entity.Property<long>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany()
            .HasForeignKey("HouseholdID")
            .HasConstraintName("FK_MealPlanEntry_Household")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Optional now, and still cascading: a plan entry pointing at a deleted recipe would be an
        // answer to "what's for dinner" that nobody can cook.
        _ = entity.Property<long?>("RecipeID");
        _ = entity.HasOne(e => e.Recipe)
            .WithMany()
            .HasForeignKey("RecipeID")
            .HasConstraintName("FK_MealPlanEntry_Recipe")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

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
