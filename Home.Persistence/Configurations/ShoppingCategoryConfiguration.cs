using Home.Domain;
using Home.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Home.Persistence.Configurations;

public class ShoppingCategoryConfiguration : IEntityTypeConfiguration<ShoppingCategory>
{

    #region Methods

    public void Configure(EntityTypeBuilder<ShoppingCategory> entity)
    {
        _ = entity.ToTable(nameof(ShoppingCategory), DomainValues.Schema);

        _ = entity.HasKey(e => e.ShoppingCategoryID);
        _ = entity.Property(e => e.ShoppingCategoryID)
            .ValueGeneratedOnAdd();

        _ = entity.Property(e => e.Name)
            .HasMaxLength(50)
            .IsRequired();

        _ = entity.Property(e => e.Sequence);

        _ = entity.Property<long>("HouseholdID");
        _ = entity.HasOne(e => e.Household)
            .WithMany(e => e.ShoppingCategories)
            .HasForeignKey("HouseholdID")
            .HasConstraintName("FK_ShoppingCategory_Household")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        _ = entity.HasIndex("HouseholdID", nameof(ShoppingCategory.Name))
            .IsUnique();
    }

    #endregion Methods

}
