using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class ShoppingItemMemoryConfiguration : IEntityTypeConfiguration<ShoppingItemMemory>
{

    #region Methods

    public void Configure(EntityTypeBuilder<ShoppingItemMemory> entity)
    {
        _ = entity.ToTable(nameof(ShoppingItemMemory), DomainValues.Schema);

        _ = entity.HasKey(e => e.ShoppingItemMemoryID);
        _ = entity.Property(e => e.ShoppingItemMemoryID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        _ = entity.Property(e => e.NameKey)
            .HasMaxLength(200)
            .IsRequired();

        _ = entity.Property<long>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany(e => e.ShoppingItemMemories)
            .HasForeignKey("HouseholdID")
            .HasConstraintName("FK_ShoppingItemMemory_Household")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // The household already cascades to both, and SQL Server refuses a second cascade path onto
        // one table, so deleting a category clears the memories filed under it itself.
        _ = entity.Property<long?>("ShoppingCategoryID");
        _ = entity.HasOne(e => e.ShoppingCategory)
            .WithMany()
            .HasForeignKey("ShoppingCategoryID")
            .HasConstraintName("FK_ShoppingItemMemory_ShoppingCategory")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        _ = entity.HasIndex("HouseholdID", nameof(ShoppingItemMemory.NameKey))
            .IsUnique();
    }

    #endregion Methods

}
