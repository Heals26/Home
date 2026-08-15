using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class RecipeMealSlotConfiguration : IEntityTypeConfiguration<RecipeMealSlot>
{

    #region Methods

    public void Configure(EntityTypeBuilder<RecipeMealSlot> entity)
    {
        _ = entity.ToTable(nameof(RecipeMealSlot), DomainValues.Schema);

        _ = entity.HasKey(e => new { e.RecipeID, e.MealSlotID });

        _ = entity.Property(e => e.RecipeID);
        _ = entity.HasOne(e => e.Recipe)
            .WithMany(e => e.MealSlots)
            .HasForeignKey(e => e.RecipeID)
            .HasConstraintName("FK_RecipeMealSlot_Recipe")
            .OnDelete(DeleteBehavior.Cascade);

        // NoAction on this side: the recipe already cascades from the household, so cascading
        // the slot too would be a second path. DeleteMealSlot clears these rows itself.
        _ = entity.Property(e => e.MealSlotID);
        _ = entity.HasOne(e => e.MealSlot)
            .WithMany(e => e.Recipes)
            .HasForeignKey(e => e.MealSlotID)
            .HasConstraintName("FK_RecipeMealSlot_MealSlot")
            .OnDelete(DeleteBehavior.NoAction);
    }

    #endregion Methods

}
